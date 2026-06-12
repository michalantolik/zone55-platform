using System.Text.Json;

namespace BlogPlatform.Cms.Seeding.Blocks;

public sealed class TextSeedBlockSerializationStrategy : ISeedBlockSerializationStrategy
{
    public string ElementTypeAlias => BlogContentAliases.TextBlock;

    public string SeedType => "text";

    public bool CanExport(string elementTypeAlias, JsonElement blockElement)
    {
        return string.Equals(elementTypeAlias, ElementTypeAlias, StringComparison.OrdinalIgnoreCase);
    }

    public bool CanImport(BlogSeedBlock block)
    {
        return string.Equals(block.Type, SeedType, StringComparison.OrdinalIgnoreCase);
    }

    public BlogSeedBlock Export(JsonElement blockElement)
    {
        return new BlogSeedBlock
        {
            Type = SeedType,
            Text = SeedBlockJson.GetString(blockElement, "text")
        };
    }

    public SeedBlock Import(BlogSeedBlock block)
    {
        return new SeedBlock(
            ElementTypeAlias,
            new Dictionary<string, object?>
            {
                ["text"] = block.Text
            });
    }
}

public sealed class HeadingSeedBlockSerializationStrategy : ISeedBlockSerializationStrategy
{
    public string ElementTypeAlias => BlogContentAliases.HeadingBlock;

    public string SeedType => "heading";

    public bool CanExport(string elementTypeAlias, JsonElement blockElement)
    {
        return string.Equals(elementTypeAlias, ElementTypeAlias, StringComparison.OrdinalIgnoreCase);
    }

    public bool CanImport(BlogSeedBlock block)
    {
        return string.Equals(block.Type, SeedType, StringComparison.OrdinalIgnoreCase);
    }

    public BlogSeedBlock Export(JsonElement blockElement)
    {
        return new BlogSeedBlock
        {
            Type = SeedType,
            Level = SeedBlockJson.GetInt(blockElement, "level"),
            Text = SeedBlockJson.GetString(blockElement, "text")
        };
    }

    public SeedBlock Import(BlogSeedBlock block)
    {
        return new SeedBlock(
            ElementTypeAlias,
            new Dictionary<string, object?>
            {
                ["level"] = block.Level ?? 2,
                ["text"] = block.Text
            });
    }
}

public sealed class CodeSnippetSeedBlockSerializationStrategy : ISeedBlockSerializationStrategy
{
    public string ElementTypeAlias => BlogContentAliases.CodeSnippetBlock;

    public string SeedType => "codeSnippet";

    public bool CanExport(string elementTypeAlias, JsonElement blockElement)
    {
        return string.Equals(elementTypeAlias, ElementTypeAlias, StringComparison.OrdinalIgnoreCase);
    }

    public bool CanImport(BlogSeedBlock block)
    {
        return string.Equals(block.Type, SeedType, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(block.Type, "code", StringComparison.OrdinalIgnoreCase);
    }

    public BlogSeedBlock Export(JsonElement blockElement)
    {
        return new BlogSeedBlock
        {
            Type = SeedType,
            Language = SeedBlockJson.GetString(blockElement, "language"),
            FileName = SeedBlockJson.GetString(blockElement, "fileName"),
            CodeTitle =
                SeedBlockJson.GetString(blockElement, "codeTitle") ??
                SeedBlockJson.GetString(blockElement, "title"),
            ShowCodeTitleBar = SeedBlockJson.GetBool(blockElement, "showCodeTitleBar"),
            Code = SeedBlockJson.GetString(blockElement, "code")
        };
    }

    public SeedBlock Import(BlogSeedBlock block)
    {
        return new SeedBlock(
            ElementTypeAlias,
            new Dictionary<string, object?>
            {
                ["language"] = block.Language,
                ["fileName"] = block.FileName,
                ["codeTitle"] = block.CodeTitle,
                ["showCodeTitleBar"] = block.ShowCodeTitleBar ?? true,
                ["code"] = block.Code
            });
    }
}

public sealed class MermaidDiagramSeedBlockSerializationStrategy : ISeedBlockSerializationStrategy
{
    public string ElementTypeAlias => BlogContentAliases.MermaidDiagramBlock;

    public string SeedType => "mermaidDiagram";

    public bool CanExport(string elementTypeAlias, JsonElement blockElement)
    {
        return string.Equals(elementTypeAlias, ElementTypeAlias, StringComparison.OrdinalIgnoreCase);
    }

    public bool CanImport(BlogSeedBlock block)
    {
        return string.Equals(block.Type, SeedType, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(block.Type, "mermaid", StringComparison.OrdinalIgnoreCase);
    }

    public BlogSeedBlock Export(JsonElement blockElement)
    {
        return new BlogSeedBlock
        {
            Type = SeedType,
            Title = SeedBlockJson.GetString(blockElement, "title"),
            ShowDiagramTitleBar = SeedBlockJson.GetBool(blockElement, "showDiagramTitleBar"),
            Diagram =
                SeedBlockJson.GetString(blockElement, "diagram") ??
                SeedBlockJson.GetString(blockElement, "mermaid")
        };
    }

    public SeedBlock Import(BlogSeedBlock block)
    {
        return new SeedBlock(
            ElementTypeAlias,
            new Dictionary<string, object?>
            {
                ["title"] = block.Title,
                ["showDiagramTitleBar"] = block.ShowDiagramTitleBar ?? true,
                ["diagram"] = block.Diagram
            });
    }
}

public sealed class PlantUmlDiagramSeedBlockSerializationStrategy : ISeedBlockSerializationStrategy
{
    public string ElementTypeAlias => BlogContentAliases.PlantUmlDiagramBlock;

    public string SeedType => "plantUmlDiagram";

    public bool CanExport(string elementTypeAlias, JsonElement blockElement)
    {
        return string.Equals(elementTypeAlias, ElementTypeAlias, StringComparison.OrdinalIgnoreCase);
    }

    public bool CanImport(BlogSeedBlock block)
    {
        return string.Equals(block.Type, SeedType, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(block.Type, "plantUml", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(block.Type, "plantuml", StringComparison.OrdinalIgnoreCase);
    }

    public BlogSeedBlock Export(JsonElement blockElement)
    {
        return new BlogSeedBlock
        {
            Type = SeedType,
            Title = SeedBlockJson.GetString(blockElement, "title"),
            ShowDiagramTitleBar = SeedBlockJson.GetBool(blockElement, "showDiagramTitleBar"),
            Diagram =
                SeedBlockJson.GetString(blockElement, "diagram") ??
                SeedBlockJson.GetString(blockElement, "plantUml") ??
                SeedBlockJson.GetString(blockElement, "plantuml")
        };
    }

    public SeedBlock Import(BlogSeedBlock block)
    {
        return new SeedBlock(
            ElementTypeAlias,
            new Dictionary<string, object?>
            {
                ["title"] = block.Title,
                ["showDiagramTitleBar"] = block.ShowDiagramTitleBar ?? true,
                ["diagram"] = block.Diagram
            });
    }
}

public sealed class CalloutSeedBlockSerializationStrategy : ISeedBlockSerializationStrategy
{
    public string ElementTypeAlias => BlogContentAliases.CalloutBlock;

    public string SeedType => "callout";

    public bool CanExport(string elementTypeAlias, JsonElement blockElement)
    {
        return string.Equals(elementTypeAlias, ElementTypeAlias, StringComparison.OrdinalIgnoreCase);
    }

    public bool CanImport(BlogSeedBlock block)
    {
        return string.Equals(block.Type, SeedType, StringComparison.OrdinalIgnoreCase);
    }

    public BlogSeedBlock Export(JsonElement blockElement)
    {
        return new BlogSeedBlock
        {
            Type = SeedType,
            Kind =
                SeedBlockJson.GetString(blockElement, "kind") ??
                SeedBlockJson.GetString(blockElement, "calloutType"),
            Text = SeedBlockJson.GetString(blockElement, "text")
        };
    }

    public SeedBlock Import(BlogSeedBlock block)
    {
        return new SeedBlock(
            ElementTypeAlias,
            new Dictionary<string, object?>
            {
                ["kind"] = block.Kind,
                ["text"] = block.Text
            });
    }
}

public sealed class SummarySeedBlockSerializationStrategy : ISeedBlockSerializationStrategy
{
    public string ElementTypeAlias => BlogContentAliases.SummaryBlock;

    public string SeedType => "summary";

    public bool CanExport(string elementTypeAlias, JsonElement blockElement)
    {
        return string.Equals(elementTypeAlias, ElementTypeAlias, StringComparison.OrdinalIgnoreCase);
    }

    public bool CanImport(BlogSeedBlock block)
    {
        return string.Equals(block.Type, SeedType, StringComparison.OrdinalIgnoreCase);
    }

    public BlogSeedBlock Export(JsonElement blockElement)
    {
        return new BlogSeedBlock
        {
            Type = SeedType,
            Summary = SeedBlockJson.GetString(blockElement, "summary")
        };
    }

    public SeedBlock Import(BlogSeedBlock block)
    {
        return new SeedBlock(
            ElementTypeAlias,
            new Dictionary<string, object?>
            {
                ["summary"] = block.Summary
            });
    }
}

public sealed class TableSeedBlockSerializationStrategy : ISeedBlockSerializationStrategy
{
    public string ElementTypeAlias => BlogContentAliases.TableBlock;

    public string SeedType => "table";

    public bool CanExport(string elementTypeAlias, JsonElement blockElement)
    {
        return string.Equals(elementTypeAlias, ElementTypeAlias, StringComparison.OrdinalIgnoreCase);
    }

    public bool CanImport(BlogSeedBlock block)
    {
        return string.Equals(block.Type, SeedType, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(block.Type, "tableBlock", StringComparison.OrdinalIgnoreCase);
    }

    public BlogSeedBlock Export(JsonElement blockElement)
    {
        return new BlogSeedBlock
        {
            Type = SeedType,
            HasHeaderRow = SeedBlockJson.GetBool(blockElement, "hasHeaderRow"),
            HasHeaderColumn = SeedBlockJson.GetBool(blockElement, "hasHeaderColumn"),
            AutoNumberRows = SeedBlockJson.GetBool(blockElement, "autoNumberRows"),
            TableStyle = SeedBlockJson.NormalizeTableStyle(
                SeedBlockJson.GetString(blockElement, "tableStyle")),
            DefaultHorizontalAlignment =
                SeedBlockJson.GetString(blockElement, "defaultHorizontalAlignment") ?? "left",
            DefaultVerticalAlignment =
                SeedBlockJson.GetString(blockElement, "defaultVerticalAlignment") ?? "middle",
            Rows = SeedBlockJson.GetTableRows(blockElement)
        };
    }

    public SeedBlock Import(BlogSeedBlock block)
    {
        return new SeedBlock(
            ElementTypeAlias,
            new Dictionary<string, object?>
            {
                ["hasHeaderRow"] = block.HasHeaderRow ?? true,
                ["hasHeaderColumn"] = block.HasHeaderColumn ?? false,
                ["autoNumberRows"] = block.AutoNumberRows ?? false,
                ["tableStyle"] = string.IsNullOrWhiteSpace(block.TableStyle)
                    ? "dense-engineering"
                    : block.TableStyle,
                ["defaultHorizontalAlignment"] = block.DefaultHorizontalAlignment ?? "left",
                ["defaultVerticalAlignment"] = block.DefaultVerticalAlignment ?? "middle",
                ["rows"] = JsonSerializer.Serialize(
                    block.Rows,
                    SeedBlockJson.TableRowsJsonOptions)
            });
    }

    public void NormalizeForUmbraco(Dictionary<string, object?> block)
    {
        SeedBlockJson.NormalizeTableRowsForUmbraco(block);
    }
}
