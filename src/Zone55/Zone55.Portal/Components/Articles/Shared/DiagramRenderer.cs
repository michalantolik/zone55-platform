using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace Zone55.Portal.Components.Articles.Shared;

public static class DiagramRenderer
{
    public static RenderFragment RenderPlantUml(
        string? diagram,
        string? title,
        bool showTitleBar,
        PlantUmlThemeValues theme) => builder =>
    {
        var sequence = 0;

        builder.OpenElement(sequence++, "figure");
        builder.AddAttribute(
            sequence++,
            "class",
            GetDiagramBlockCssClass(showTitleBar, "plantuml-block"));

        if (showTitleBar)
        {
            builder.OpenElement(sequence++, "figcaption");
            builder.AddContent(sequence++, GetPlantUmlDiagramTitle(diagram, title));
            builder.CloseElement();
        }

        builder.OpenElement(sequence++, "div");
        builder.AddAttribute(sequence++, "class", "diagram-render-surface");

        builder.OpenComponent<PlantUmlDiagramImage>(sequence++);
        builder.AddAttribute(sequence++, nameof(PlantUmlDiagramImage.Source), CreatePlantUmlUrl(diagram, theme));
        builder.AddAttribute(sequence++, nameof(PlantUmlDiagramImage.AlternativeText), GetPlantUmlDiagramTitle(diagram, title));
        builder.CloseComponent();
        builder.CloseElement();
        builder.CloseElement();
    };

    public static RenderFragment RenderMermaid(
        string? diagram,
        string? title,
        bool showTitleBar) => builder =>
    {
        var sequence = 0;

        builder.OpenElement(sequence++, "figure");
        builder.AddAttribute(
            sequence++,
            "class",
            GetDiagramBlockCssClass(showTitleBar, "mermaid-block"));

        if (showTitleBar)
        {
            builder.OpenElement(sequence++, "figcaption");
            builder.AddContent(sequence++, GetMermaidDiagramTitle(diagram, title));
            builder.CloseElement();
        }

        builder.OpenElement(sequence++, "pre");
        builder.AddAttribute(sequence++, "class", "mermaid");
        builder.AddAttribute(sequence++, "data-mermaid-source", diagram);
        builder.AddContent(sequence++, diagram);
        builder.CloseElement();

        builder.CloseElement();
    };

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

    public static string GetDiagramBlockCssClass(
        bool showTitleBar,
        string diagramTypeClass)
    {
        return showTitleBar
            ? $"diagram-block {diagramTypeClass}"
            : $"diagram-block {diagramTypeClass} diagram-block-without-title";
    }

    public static string GetMermaidDiagramTitle(
        string? diagram,
        string? title)
    {
        return GetDiagramTitle(diagram, title, "Mermaid diagram");
    }

    public static string GetPlantUmlDiagramTitle(
        string? diagram,
        string? title)
    {
        return GetDiagramTitle(diagram, title, "PlantUML diagram");
    }

    private static string GetDiagramTitle(
        string? diagram,
        string? title,
        string fallback)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            return title.Trim();
        }

        var titleFromPlantUmlSource = ExtractPlantUmlTitle(diagram);

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

        var skinparams = Zone55PlantUmlTheme.CreateSkinParams(theme);

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

    internal static string EncodePlantUml(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var sourceBytes = Encoding.UTF8.GetBytes(source);
        using var output = new MemoryStream();

        // PlantUML server URLs use raw DEFLATE followed by PlantUML's own
        // URL-safe 6-bit alphabet. This is substantially shorter than ~h HEX
        // payloads and matches the encoding expected by the public server.
        using (var deflate = new DeflateStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            deflate.Write(sourceBytes, 0, sourceBytes.Length);
        }

        return Encode64(output.ToArray());
    }

    private static string Encode64(byte[] data)
    {
        var result = new StringBuilder((data.Length * 4 + 2) / 3);

        for (var index = 0; index < data.Length; index += 3)
        {
            var remaining = data.Length - index;
            Append3Bytes(
                result,
                data[index],
                remaining > 1 ? data[index + 1] : (byte)0,
                remaining > 2 ? data[index + 2] : (byte)0,
                Math.Min(remaining, 3));
        }

        return result.ToString();
    }

    private static void Append3Bytes(
        StringBuilder result,
        byte first,
        byte second,
        byte third,
        int count)
    {
        var c1 = first >> 2;
        var c2 = ((first & 0x3) << 4) | (second >> 4);
        var c3 = ((second & 0xF) << 2) | (third >> 6);
        var c4 = third & 0x3F;

        result.Append(Encode6Bit(c1));
        result.Append(Encode6Bit(c2));

        if (count > 1)
        {
            result.Append(Encode6Bit(c3));
        }

        if (count > 2)
        {
            result.Append(Encode6Bit(c4));
        }
    }

    private static char Encode6Bit(int value)
    {
        if (value < 10)
        {
            return (char)('0' + value);
        }

        value -= 10;
        if (value < 26)
        {
            return (char)('A' + value);
        }

        value -= 26;
        if (value < 26)
        {
            return (char)('a' + value);
        }

        value -= 26;
        return value switch
        {
            0 => '-',
            1 => '_',
            _ => '?'
        };
    }

    private static string RemoveProtectedPlantUmlSkinParams(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

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

            if (Zone55PlantUmlTheme.ProtectedSingleParams.Contains(firstToken))
            {
                continue;
            }

            if (Zone55PlantUmlTheme.ProtectedBlockNames.Contains(firstToken) && skinparamBody.Contains('{', StringComparison.Ordinal))
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
