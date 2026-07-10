using System.Text.Json;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization.PreviewBlocks;

internal abstract class ArticleBlockDefinitionBase : IPreviewArticleBlockDefinition
{
    protected ArticleBlockDefinitionBase(
        PreviewArticleBlockType type,
        string displayName,
        string editorType,
        params string[] umbracoAliases)
    {
        Type = type;
        DisplayName = displayName;
        EditorType = editorType;
        UmbracoAliases = umbracoAliases;
    }

    public PreviewArticleBlockType Type { get; }

    public string DisplayName { get; }

    public string EditorType { get; }

    public IReadOnlyCollection<string> UmbracoAliases { get; }

    public abstract bool CanParse(JsonElement block);

    public abstract PreviewArticleBlock Parse(JsonElement block);

    protected static string? Text(JsonElement block)
    {
        return PreviewArticleBlockJsonReader.GetString(block, PreviewArticleBlockPropertyNames.Text);
    }
}

internal sealed class CodeArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public CodeArticleBlockDefinition()
        : base(PreviewArticleBlockType.Code, "Code snippet", "code", "codeSnippetBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return PreviewArticleBlockJsonReader.HasString(block, PreviewArticleBlockPropertyNames.Code);
    }

    public override PreviewArticleBlock Parse(JsonElement block)
    {
        return new PreviewArticleBlock(PreviewArticleBlockType.Code)
        {
            Code = PreviewArticleBlockJsonReader.GetString(block, PreviewArticleBlockPropertyNames.Code),
            Language = PreviewArticleBlockJsonReader.GetString(block, PreviewArticleBlockPropertyNames.Language),
            FileName = PreviewArticleBlockJsonReader.GetString(block, PreviewArticleBlockPropertyNames.FileName),
            CodeTitle = PreviewArticleBlockJsonReader.GetFirstString(
                block,
                PreviewArticleBlockPropertyNames.CodeTitle,
                PreviewArticleBlockPropertyNames.Title,
                PreviewArticleBlockPropertyNames.FileName,
                PreviewArticleBlockPropertyNames.Language),
            ShowCodeTitleBar = PreviewArticleBlockJsonReader.GetBool(
                block,
                PreviewArticleBlockPropertyNames.ShowCodeTitleBar) ?? true
        };
    }
}

internal sealed class PlantUmlArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public PlantUmlArticleBlockDefinition()
        : base(
            PreviewArticleBlockType.PlantUml,
            "PlantUML diagram",
            "plantuml",
            "plantUmlDiagramBlock",
            "plantumlDiagramBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        if (PreviewArticleBlockJsonReader.HasAnyString(
                block,
                PreviewArticleBlockPropertyNames.PlantUml,
                PreviewArticleBlockPropertyNames.Plantuml))
        {
            return true;
        }

        var diagram = PreviewArticleBlockJsonReader.GetString(block, PreviewArticleBlockPropertyNames.Diagram);

        return IsPlantUmlSource(diagram);
    }

    public override PreviewArticleBlock Parse(JsonElement block)
    {
        return new PreviewArticleBlock(PreviewArticleBlockType.PlantUml)
        {
            Diagram = PreviewArticleBlockJsonReader.GetFirstString(
                block,
                PreviewArticleBlockPropertyNames.PlantUml,
                PreviewArticleBlockPropertyNames.Plantuml,
                PreviewArticleBlockPropertyNames.Diagram),
            DiagramTitle = PreviewArticleBlockJsonReader.GetFirstString(
                block,
                PreviewArticleBlockPropertyNames.Title,
                PreviewArticleBlockPropertyNames.DiagramTitle),
            ShowDiagramTitleBar = PreviewArticleBlockJsonReader.GetBool(
                block,
                PreviewArticleBlockPropertyNames.ShowDiagramTitleBar) ?? true
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
        : base(PreviewArticleBlockType.Mermaid, "Mermaid diagram", "mermaid", "mermaidDiagramBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return PreviewArticleBlockJsonReader.HasAnyString(
            block,
            PreviewArticleBlockPropertyNames.Mermaid,
            PreviewArticleBlockPropertyNames.Diagram);
    }

    public override PreviewArticleBlock Parse(JsonElement block)
    {
        return new PreviewArticleBlock(PreviewArticleBlockType.Mermaid)
        {
            Diagram = PreviewArticleBlockJsonReader.GetFirstString(
                block,
                PreviewArticleBlockPropertyNames.Mermaid,
                PreviewArticleBlockPropertyNames.Diagram),
            DiagramTitle = PreviewArticleBlockJsonReader.GetFirstString(
                block,
                PreviewArticleBlockPropertyNames.Title,
                PreviewArticleBlockPropertyNames.DiagramTitle),
            ShowDiagramTitleBar = PreviewArticleBlockJsonReader.GetBool(
                block,
                PreviewArticleBlockPropertyNames.ShowDiagramTitleBar) ?? true
        };
    }
}

internal sealed class TableArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public TableArticleBlockDefinition()
        : base(PreviewArticleBlockType.Table, "Table", "table", "tableBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return PreviewArticleBlockJsonReader.HasProperty(block, PreviewArticleBlockPropertyNames.Rows);
    }

    public override PreviewArticleBlock Parse(JsonElement block)
    {
        var rows = block.TryGetProperty(PreviewArticleBlockPropertyNames.Rows, out var rowsElement)
            ? ParseTableRowsProperty(rowsElement)
            : [];

        return new PreviewArticleBlock(PreviewArticleBlockType.Table)
        {
            TableOptions = new PreviewArticleTableOptions
            {
                HasHeaderRow = PreviewArticleBlockJsonReader.GetBool(block, PreviewArticleBlockPropertyNames.HasHeaderRow) ?? true,
                HasHeaderColumn = PreviewArticleBlockJsonReader.GetBool(block, PreviewArticleBlockPropertyNames.HasHeaderColumn) ?? false,
                AutoNumberRows = PreviewArticleBlockJsonReader.GetBool(block, PreviewArticleBlockPropertyNames.AutoNumberRows) ?? false,
                TableStyle = NormalizeTableStyle(
                    PreviewArticleBlockJsonReader.GetString(block, PreviewArticleBlockPropertyNames.TableStyle)),
                DefaultHorizontalAlignment = PreviewArticleBlockJsonReader.GetString(
                    block,
                    PreviewArticleBlockPropertyNames.DefaultHorizontalAlignment) ?? "left",
                DefaultVerticalAlignment = PreviewArticleBlockJsonReader.GetString(
                    block,
                    PreviewArticleBlockPropertyNames.DefaultVerticalAlignment) ?? "middle"
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

    private static IReadOnlyList<IReadOnlyList<PreviewArticleTableCell>> ParseTableRowsProperty(
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

    private static IReadOnlyList<IReadOnlyList<PreviewArticleTableCell>> ParseTableRows(
        JsonElement rowsElement)
    {
        return rowsElement
            .EnumerateArray()
            .Where(row => row.ValueKind == JsonValueKind.Array)
            .Select(row => row
                .EnumerateArray()
                .Select(ParseTableCell)
                .ToArray() as IReadOnlyList<PreviewArticleTableCell>)
            .ToArray();
    }

    private static PreviewArticleTableCell ParseTableCell(JsonElement cell)
    {
        if (cell.ValueKind == JsonValueKind.String)
        {
            return new PreviewArticleTableCell
            {
                Text = cell.GetString()
            };
        }

        return new PreviewArticleTableCell
        {
            Text = PreviewArticleBlockJsonReader.GetString(cell, PreviewArticleBlockPropertyNames.Text),
            Emoji = PreviewArticleBlockJsonReader.GetString(cell, PreviewArticleBlockPropertyNames.Emoji),
            ImageUrl = PreviewArticleBlockJsonReader.GetString(cell, PreviewArticleBlockPropertyNames.ImageUrl),
            ImageAlt = PreviewArticleBlockJsonReader.GetString(cell, PreviewArticleBlockPropertyNames.ImageAlt),
            HorizontalAlignment = PreviewArticleBlockJsonReader.GetString(cell, PreviewArticleBlockPropertyNames.HorizontalAlignment),
            VerticalAlignment = PreviewArticleBlockJsonReader.GetString(cell, PreviewArticleBlockPropertyNames.VerticalAlignment)
        };
    }
}

internal sealed class SummaryArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public SummaryArticleBlockDefinition()
        : base(PreviewArticleBlockType.Summary, "Summary", "summary", "summaryBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return PreviewArticleBlockJsonReader.HasString(block, PreviewArticleBlockPropertyNames.Summary);
    }

    public override PreviewArticleBlock Parse(JsonElement block)
    {
        var summary = PreviewArticleBlockJsonReader.GetString(block, PreviewArticleBlockPropertyNames.Summary);

        return new PreviewArticleBlock(PreviewArticleBlockType.Summary)
        {
            Summary = summary,
            Text = summary
        };
    }
}

internal sealed class CalloutArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public CalloutArticleBlockDefinition()
        : base(PreviewArticleBlockType.Callout, "Callout", "callout", "calloutBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return PreviewArticleBlockJsonReader.HasAnyString(
            block,
            PreviewArticleBlockPropertyNames.Kind,
            PreviewArticleBlockPropertyNames.CalloutType);
    }

    public override PreviewArticleBlock Parse(JsonElement block)
    {
        return new PreviewArticleBlock(PreviewArticleBlockType.Callout)
        {
            Kind = PreviewArticleBlockJsonReader
                .GetFirstString(
                    block,
                    PreviewArticleBlockPropertyNames.Kind,
                    PreviewArticleBlockPropertyNames.CalloutType)
                ?.ToLowerInvariant(),
            Text = Text(block)
        };
    }
}

internal sealed class HeadingArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public HeadingArticleBlockDefinition()
        : base(PreviewArticleBlockType.Heading, "Heading", "heading", "headingBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return PreviewArticleBlockJsonReader.HasProperty(block, PreviewArticleBlockPropertyNames.Level);
    }

    public override PreviewArticleBlock Parse(JsonElement block)
    {
        var level = PreviewArticleBlockJsonReader.GetInt(block, PreviewArticleBlockPropertyNames.Level) ?? 2;

        return new PreviewArticleBlock(PreviewArticleBlockType.Heading)
        {
            Level = Math.Clamp(level, 2, 4),
            Text = Text(block)
        };
    }
}

internal sealed class TextArticleBlockDefinition : ArticleBlockDefinitionBase
{
    public TextArticleBlockDefinition()
        : base(PreviewArticleBlockType.Text, "Text", "text", "textBlock")
    {
    }

    public override bool CanParse(JsonElement block)
    {
        return true;
    }

    public override PreviewArticleBlock Parse(JsonElement block)
    {
        return new PreviewArticleBlock(PreviewArticleBlockType.Text)
        {
            Text = Text(block)
        };
    }
}
