using System.Text.Json;

namespace BlogPlatform.Contracts.Posts;

public static class ArticleBlockParser
{
    public static IReadOnlyCollection<ArticleBlockDto> Parse(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return [];
        }

        using var document = JsonDocument.Parse(rawJson);

        var root = document.RootElement;

        if (!root.TryGetProperty("layout", out var layout) ||
            !layout.TryGetProperty("Umbraco.BlockList", out var layoutItems) ||
            !root.TryGetProperty("contentData", out var contentData))
        {
            return [];
        }

        var contentDataByUdi = contentData
            .EnumerateArray()
            .Where(x => x.TryGetProperty("udi", out _))
            .ToDictionary(
                x => x.GetProperty("udi").GetString() ?? string.Empty,
                x => x);

        var blocks = new List<ArticleBlockDto>();

        foreach (var layoutItem in layoutItems.EnumerateArray())
        {
            var contentUdi = layoutItem.GetProperty("contentUdi").GetString();

            if (string.IsNullOrWhiteSpace(contentUdi) ||
                !contentDataByUdi.TryGetValue(contentUdi, out var block))
            {
                continue;
            }

            blocks.Add(ParseBlock(block));
        }

        return blocks;
    }

    private static ArticleBlockDto ParseBlock(JsonElement block)
    {
        var text = GetString(block, "text");
        var title = GetString(block, "title") ?? GetString(block, "diagramTitle");
        var level = GetInt(block, "level") ?? 2;
        var code = GetString(block, "code");
        var diagram = GetString(block, "diagram");
        var mermaid = GetString(block, "mermaid");
        var plantUml = GetString(block, "plantUml") ?? GetString(block, "plantuml");
        var kind = GetString(block, "kind") ?? GetString(block, "calloutType");
        var language = GetString(block, "language");
        var fileName = GetString(block, "fileName");
        var showDiagramTitleBar = GetBool(block, "showDiagramTitleBar") ?? true;

        if (!string.IsNullOrWhiteSpace(code))
        {
            return new ArticleBlockDto(ArticleBlockType.Code)
            {
                Code = code,
                Language = language,
                FileName = fileName
            };
        }

        if (!string.IsNullOrWhiteSpace(plantUml))
        {
            return new ArticleBlockDto(ArticleBlockType.PlantUml)
            {
                Diagram = plantUml,
                DiagramTitle = title,
                ShowDiagramTitleBar = showDiagramTitleBar
            };
        }

        if (!string.IsNullOrWhiteSpace(mermaid))
        {
            return new ArticleBlockDto(ArticleBlockType.Mermaid)
            {
                Diagram = mermaid,
                DiagramTitle = title,
                ShowDiagramTitleBar = showDiagramTitleBar
            };
        }

        if (!string.IsNullOrWhiteSpace(diagram))
        {
            return diagram.TrimStart().StartsWith("@startuml", StringComparison.OrdinalIgnoreCase)
                ? new ArticleBlockDto(ArticleBlockType.PlantUml)
                {
                    Diagram = diagram,
                    DiagramTitle = title,
                ShowDiagramTitleBar = showDiagramTitleBar
                }
                : new ArticleBlockDto(ArticleBlockType.Mermaid)
                {
                    Diagram = diagram,
                    DiagramTitle = title,
                ShowDiagramTitleBar = showDiagramTitleBar
                };
        }

        if (!string.IsNullOrWhiteSpace(kind))
        {
            return new ArticleBlockDto(ArticleBlockType.Callout)
            {
                Kind = kind.ToLowerInvariant(),
                Text = text
            };
        }

        if (block.TryGetProperty("level", out _))
        {
            return new ArticleBlockDto(ArticleBlockType.Heading)
            {
                Level = Math.Clamp(level, 2, 4),
                Text = text
            };
        }

        return new ArticleBlockDto(ArticleBlockType.Text)
        {
            Text = text
        };
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value)
            ? value.GetString()
            : null;
    }

    private static bool? GetBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) &&
               value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;
    }

    private static int? GetInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) &&
               value.TryGetInt32(out var number)
            ? number
            : null;
    }
}
