using System.Text.Json;

namespace BlogPlatform.Contracts.Posts.ArticleBlocks;

internal abstract class ArticleBlockDefinitionBase : IArticleBlockDefinition
{
    protected ArticleBlockDefinitionBase(
        ArticleBlockType type,
        string displayName,
        string editorType,
        params string[] umbracoAliases)
    {
        Type = type;
        DisplayName = displayName;
        EditorType = editorType;
        UmbracoAliases = umbracoAliases;
    }

    public ArticleBlockType Type { get; }

    public string DisplayName { get; }

    public string EditorType { get; }

    public IReadOnlyCollection<string> UmbracoAliases { get; }

    public abstract bool CanParse(JsonElement block);

    public abstract ArticleBlockDto Parse(JsonElement block);

    protected static string? Text(JsonElement block)
    {
        return ArticleBlockJsonReader.GetString(block, ArticleBlockPropertyNames.Text);
    }
}

internal sealed class CodeArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public CodeArticleBlockDefinition()
        : base(ArticleBlockType.Code, "Code snippet", "code", "codeSnippetBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return ArticleBlockJsonReader.HasString(block, ArticleBlockPropertyNames.Code);
    }

    public override ArticleBlockDto Parse(JsonElement block)
    {
        return new ArticleBlockDto(ArticleBlockType.Code)
        {
            Code = ArticleBlockJsonReader.GetString(block, ArticleBlockPropertyNames.Code),
            Language = ArticleBlockJsonReader.GetString(block, ArticleBlockPropertyNames.Language),
            FileName = ArticleBlockJsonReader.GetString(block, ArticleBlockPropertyNames.FileName),
            CodeTitle = ArticleBlockJsonReader.GetFirstString(
                block,
                ArticleBlockPropertyNames.CodeTitle,
                ArticleBlockPropertyNames.Title,
                ArticleBlockPropertyNames.FileName,
                ArticleBlockPropertyNames.Language),
            ShowCodeTitleBar = ArticleBlockJsonReader.GetBool(
                block,
                ArticleBlockPropertyNames.ShowCodeTitleBar) ?? true
        };
    }
}

internal sealed class PlantUmlArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public PlantUmlArticleBlockDefinition()
        : base(
            ArticleBlockType.PlantUml,
            "PlantUML diagram",
            "plantuml",
            "plantUmlDiagramBlock",
            "plantumlDiagramBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        if (ArticleBlockJsonReader.HasAnyString(
                block,
                ArticleBlockPropertyNames.PlantUml,
                ArticleBlockPropertyNames.Plantuml))
        {
            return true;
        }

        var diagram = ArticleBlockJsonReader.GetString(block, ArticleBlockPropertyNames.Diagram);

        return IsPlantUmlSource(diagram);
    }

    public override ArticleBlockDto Parse(JsonElement block)
    {
        return new ArticleBlockDto(ArticleBlockType.PlantUml)
        {
            Diagram = ArticleBlockJsonReader.GetFirstString(
                block,
                ArticleBlockPropertyNames.PlantUml,
                ArticleBlockPropertyNames.Plantuml,
                ArticleBlockPropertyNames.Diagram),
            DiagramTitle = ArticleBlockJsonReader.GetFirstString(
                block,
                ArticleBlockPropertyNames.Title,
                ArticleBlockPropertyNames.DiagramTitle),
            ShowDiagramTitleBar = ArticleBlockJsonReader.GetBool(
                block,
                ArticleBlockPropertyNames.ShowDiagramTitleBar) ?? true
        };
    }

    private static bool IsPlantUmlSource(string? diagram)
    {
        return !string.IsNullOrWhiteSpace(diagram) &&
               diagram.TrimStart().StartsWith("@startuml", StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed class MermaidArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public MermaidArticleBlockDefinition()
        : base(ArticleBlockType.Mermaid, "Mermaid diagram", "mermaid", "mermaidDiagramBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return ArticleBlockJsonReader.HasAnyString(
            block,
            ArticleBlockPropertyNames.Mermaid,
            ArticleBlockPropertyNames.Diagram);
    }

    public override ArticleBlockDto Parse(JsonElement block)
    {
        return new ArticleBlockDto(ArticleBlockType.Mermaid)
        {
            Diagram = ArticleBlockJsonReader.GetFirstString(
                block,
                ArticleBlockPropertyNames.Mermaid,
                ArticleBlockPropertyNames.Diagram),
            DiagramTitle = ArticleBlockJsonReader.GetFirstString(
                block,
                ArticleBlockPropertyNames.Title,
                ArticleBlockPropertyNames.DiagramTitle),
            ShowDiagramTitleBar = ArticleBlockJsonReader.GetBool(
                block,
                ArticleBlockPropertyNames.ShowDiagramTitleBar) ?? true
        };
    }
}

internal sealed class TableArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public TableArticleBlockDefinition()
        : base(ArticleBlockType.Table, "Table", "table", "tableBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return ArticleBlockJsonReader.HasProperty(block, ArticleBlockPropertyNames.Rows);
    }

    public override ArticleBlockDto Parse(JsonElement block)
    {
        var rows = block.TryGetProperty(ArticleBlockPropertyNames.Rows, out var rowsElement)
            ? ParseTableRowsProperty(rowsElement)
            : [];

        return new ArticleBlockDto(ArticleBlockType.Table)
        {
            TableOptions = new ArticleTableOptionsDto
            {
                HasHeaderRow = ArticleBlockJsonReader.GetBool(block, ArticleBlockPropertyNames.HasHeaderRow) ?? true,
                HasHeaderColumn = ArticleBlockJsonReader.GetBool(block, ArticleBlockPropertyNames.HasHeaderColumn) ?? false,
                AutoNumberRows = ArticleBlockJsonReader.GetBool(block, ArticleBlockPropertyNames.AutoNumberRows) ?? false,
                TableStyle = NormalizeTableStyle(
                    ArticleBlockJsonReader.GetString(block, ArticleBlockPropertyNames.TableStyle)),
                DefaultHorizontalAlignment = ArticleBlockJsonReader.GetString(
                    block,
                    ArticleBlockPropertyNames.DefaultHorizontalAlignment) ?? "left",
                DefaultVerticalAlignment = ArticleBlockJsonReader.GetString(
                    block,
                    ArticleBlockPropertyNames.DefaultVerticalAlignment) ?? "middle"
            },
            TableRows = rows
        };
    }

    private static string NormalizeTableStyle(string? value)
    {
        return string.Equals(value, "minimal-reference", StringComparison.OrdinalIgnoreCase)
            ? "minimal-reference"
            : "dense-engineering";
    }

    private static IReadOnlyList<IReadOnlyList<ArticleTableCellDto>> ParseTableRowsProperty(
        JsonElement rowsElement)
    {
        if (rowsElement.ValueKind == JsonValueKind.Array)
        {
            return ParseTableRows(rowsElement);
        }

        if (rowsElement.ValueKind != JsonValueKind.String)
        {
            return [];
        }

        var rawRowsJson = rowsElement.GetString();

        if (string.IsNullOrWhiteSpace(rawRowsJson))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(rawRowsJson);

            return document.RootElement.ValueKind == JsonValueKind.Array
                ? ParseTableRows(document.RootElement)
                : [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static IReadOnlyList<IReadOnlyList<ArticleTableCellDto>> ParseTableRows(
        JsonElement rowsElement)
    {
        return rowsElement
            .EnumerateArray()
            .Where(row => row.ValueKind == JsonValueKind.Array)
            .Select(row => row
                .EnumerateArray()
                .Select(ParseTableCell)
                .ToArray() as IReadOnlyList<ArticleTableCellDto>)
            .ToArray();
    }

    private static ArticleTableCellDto ParseTableCell(JsonElement cell)
    {
        if (cell.ValueKind == JsonValueKind.String)
        {
            return new ArticleTableCellDto
            {
                Text = cell.GetString()
            };
        }

        return new ArticleTableCellDto
        {
            Text = ArticleBlockJsonReader.GetString(cell, ArticleBlockPropertyNames.Text),
            Emoji = ArticleBlockJsonReader.GetString(cell, ArticleBlockPropertyNames.Emoji),
            ImageUrl = ArticleBlockJsonReader.GetString(cell, ArticleBlockPropertyNames.ImageUrl),
            ImageAlt = ArticleBlockJsonReader.GetString(cell, ArticleBlockPropertyNames.ImageAlt),
            HorizontalAlignment = ArticleBlockJsonReader.GetString(cell, ArticleBlockPropertyNames.HorizontalAlignment),
            VerticalAlignment = ArticleBlockJsonReader.GetString(cell, ArticleBlockPropertyNames.VerticalAlignment)
        };
    }
}

internal sealed class SummaryArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public SummaryArticleBlockDefinition()
        : base(ArticleBlockType.Summary, "Summary", "summary", "summaryBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return ArticleBlockJsonReader.HasString(block, ArticleBlockPropertyNames.Summary);
    }

    public override ArticleBlockDto Parse(JsonElement block)
    {
        var summary = ArticleBlockJsonReader.GetString(block, ArticleBlockPropertyNames.Summary);

        return new ArticleBlockDto(ArticleBlockType.Summary)
        {
            Summary = summary,
            Text = summary
        };
    }
}

internal sealed class CalloutArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public CalloutArticleBlockDefinition()
        : base(ArticleBlockType.Callout, "Callout", "callout", "calloutBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return ArticleBlockJsonReader.HasAnyString(
            block,
            ArticleBlockPropertyNames.Kind,
            ArticleBlockPropertyNames.CalloutType);
    }

    public override ArticleBlockDto Parse(JsonElement block)
    {
        return new ArticleBlockDto(ArticleBlockType.Callout)
        {
            Kind = ArticleBlockJsonReader
                .GetFirstString(
                    block,
                    ArticleBlockPropertyNames.Kind,
                    ArticleBlockPropertyNames.CalloutType)
                ?.ToLowerInvariant(),
            Text = Text(block)
        };
    }
}

internal sealed class HeadingArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public HeadingArticleBlockDefinition()
        : base(ArticleBlockType.Heading, "Heading", "heading", "headingBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return ArticleBlockJsonReader.HasProperty(block, ArticleBlockPropertyNames.Level);
    }

    public override ArticleBlockDto Parse(JsonElement block)
    {
        var level = ArticleBlockJsonReader.GetInt(block, ArticleBlockPropertyNames.Level) ?? 2;

        return new ArticleBlockDto(ArticleBlockType.Heading)
        {
            Level = Math.Clamp(level, 2, 4),
            Text = Text(block)
        };
    }
}

internal sealed class TextArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public TextArticleBlockDefinition()
        : base(ArticleBlockType.Text, "Text", "text", "textBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return true;
    }

    public override ArticleBlockDto Parse(JsonElement block)
    {
        return new ArticleBlockDto(ArticleBlockType.Text)
        {
            Text = Text(block)
        };
    }
}
