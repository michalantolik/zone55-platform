using System.IO.Compression;
using System.Text;
using BlogPlatform.App.Components.Articles.Rendering.Context;
using BlogPlatform.Contracts.Posts;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Diagram;

public static class ArticleBlockDiagramRenderer
{
    public static string GetDiagramBlockCssClass(
        ArticleBlockDto block,
        string diagramTypeClass)
    {
        return block.ShowDiagramTitleBar
            ? $"diagram-block {diagramTypeClass}"
            : $"diagram-block {diagramTypeClass} diagram-block-without-title";
    }

    public static string GetMermaidDiagramTitle(ArticleBlockDto block)
    {
        return GetDiagramTitle(block, "Mermaid diagram");
    }

    public static string GetPlantUmlDiagramTitle(ArticleBlockDto block)
    {
        return GetDiagramTitle(block, "PlantUML diagram");
    }

    public static string CreatePlantUmlUrl(
        string? diagram,
        PlantUmlThemeValues theme)
    {
        if (string.IsNullOrWhiteSpace(diagram))
        {
            return string.Empty;
        }

        return $"https://www.plantuml.com/plantuml/svg/{EncodePlantUml(CreateThemeAwarePlantUmlSource(diagram, theme))}";
    }

    private static string GetDiagramTitle(
        ArticleBlockDto block,
        string fallback)
    {
        if (!string.IsNullOrWhiteSpace(block.DiagramTitle))
        {
            return block.DiagramTitle.Trim();
        }

        var titleFromPlantUmlSource = ExtractPlantUmlTitle(block.Diagram);

        if (!string.IsNullOrWhiteSpace(titleFromPlantUmlSource))
        {
            return titleFromPlantUmlSource;
        }

        return fallback;
    }

    private static string? ExtractPlantUmlTitle(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        var titleLine = source
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select(line => line.Trim())
            .FirstOrDefault(line => line.StartsWith("title ", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(titleLine))
        {
            return null;
        }

        return titleLine["title ".Length..].Trim();
    }

    private static string CreateThemeAwarePlantUmlSource(
        string source,
        PlantUmlThemeValues theme)
    {
        var normalizedSource = RemoveProtectedPlantUmlSkinParams(source).Trim();

        var skinparams = CreatePlantUmlSkinParams(theme);

        if (!normalizedSource.Contains("@start", StringComparison.OrdinalIgnoreCase))
        {
            return $"@startuml\n{skinparams}\n{normalizedSource}\n@enduml";
        }

        var firstLineBreakIndex = normalizedSource.IndexOf('\n', StringComparison.Ordinal);

        if (firstLineBreakIndex < 0)
        {
            return $"{normalizedSource}\n{skinparams}";
        }

        return normalizedSource.Insert(firstLineBreakIndex + 1, $"{skinparams}\n");
    }

    private static string CreatePlantUmlSkinParams(PlantUmlThemeValues theme)
    {
        var lineColor = NormalizePlantUmlColor(theme.Line, "#a7b4c8");
        var nodeTextColor = NormalizePlantUmlColor(theme.NodeText, "#E5E7EB");
        var nodeBorderColor = NormalizePlantUmlColor(theme.NodeBorder, "#7d8ca3");
        var nodeBgColor = NormalizePlantUmlColor(theme.NodeBg, "#374151");
        var fontFamily = NormalizePlantUmlFontFamily(theme.FontFamily);

        return $$$"""
skinparam backgroundColor transparent
skinparam shadowing false
skinparam defaultFontName {{{fontFamily}}}
skinparam defaultFontColor {{{nodeTextColor}}}
skinparam ArrowColor {{{lineColor}}}
skinparam ArrowFontColor {{{lineColor}}}
skinparam ArrowThickness 1
skinparam rectangle {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{nodeBorderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam class {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{nodeBorderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam component {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{nodeBorderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam package {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{nodeBorderColor}}}
    FontColor {{{nodeTextColor}}}
}
""";
    }

    private static string NormalizePlantUmlColor(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("#", StringComparison.Ordinal) ||
            trimmed.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return fallback;
    }

    private static string NormalizePlantUmlFontFamily(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "SansSerif";
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? "SansSerif";
    }

    private static string EncodePlantUml(string source)
    {
        var bytes = Encoding.UTF8.GetBytes(source);

        using var output = new MemoryStream();

        using (var deflate = new DeflateStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            deflate.Write(bytes, 0, bytes.Length);
        }

        return EncodePlantUmlBytes(output.ToArray());
    }

    private static string EncodePlantUmlBytes(byte[] data)
    {
        const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";

        var result = new StringBuilder();

        for (var i = 0; i < data.Length; i += 3)
        {
            var b1 = data[i];
            var b2 = i + 1 < data.Length ? data[i + 1] : 0;
            var b3 = i + 2 < data.Length ? data[i + 2] : 0;

            result.Append(alphabet[b1 >> 2]);
            result.Append(alphabet[((b1 & 0x3) << 4) | (b2 >> 4)]);
            result.Append(alphabet[((b2 & 0xF) << 2) | (b3 >> 6)]);
            result.Append(alphabet[b3 & 0x3F]);
        }

        return result.ToString();
    }

    private static string RemoveProtectedPlantUmlSkinParams(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

        var protectedBlockNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "rectangle",
            "class",
            "component",
            "package"
        };

        var protectedSingleParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "backgroundColor",
            "shadowing",
            "defaultFontName",
            "defaultFontColor",
            "ArrowColor",
            "ArrowFontColor",
            "ArrowThickness"
        };

        var lines = source.Replace("\r\n", "\n").Split('\n');
        var result = new List<string>();
        var skippingProtectedBlock = false;
        var protectedBlockDepth = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (skippingProtectedBlock)
            {
                protectedBlockDepth += trimmed.Count(character => character == '{');
                protectedBlockDepth -= trimmed.Count(character => character == '}');

                if (protectedBlockDepth <= 0)
                {
                    skippingProtectedBlock = false;
                }

                continue;
            }

            if (!trimmed.StartsWith("skinparam ", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(line);
                continue;
            }

            var skinparamBody = trimmed["skinparam ".Length..].Trim();

            var firstToken = skinparamBody
                .Split([' ', '\t', '{'], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (firstToken is null)
            {
                continue;
            }

            if (protectedSingleParams.Contains(firstToken))
            {
                continue;
            }

            if (protectedBlockNames.Contains(firstToken) && skinparamBody.Contains('{', StringComparison.Ordinal))
            {
                skippingProtectedBlock = true;
                protectedBlockDepth = 1;
                continue;
            }

            result.Add(line);
        }

        return string.Join('\n', result);
    }
}
