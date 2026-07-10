using System.Text.Json;
using BlogPlatform.App.Components.Articles.LearnKitRendering.Helpers;
using BlogPlatform.App.Components.Articles.LearnKitRendering.Models;
using BlogPlatform.App.Models.LearnKit;
using BlogPlatform.App.Models.LearnKit.Articles;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization;

public static class LearnKitBlockDeserializer
{
    public static MarkdownBlockModel ToMarkdownBlock(
        LearnKitArticleBlockDetails block)
    {
        var json = LearnKitBlockJsonHelper.Parse(block.ContentJson);

        var markdown =
            LearnKitBlockJsonHelper.GetString(json, "markdown") ??
            LearnKitBlockJsonHelper.GetString(json, "text") ??
            string.Empty;

        return new MarkdownBlockModel(markdown);
    }

    public static SummaryBlockModel ToSummaryBlock(
        LearnKitArticleBlockDetails block)
    {
        var json = LearnKitBlockJsonHelper.Parse(block.ContentJson);

        var summary =
            LearnKitBlockJsonHelper.GetString(json, "summary") ??
            LearnKitBlockJsonHelper.GetString(json, "text") ??
            string.Empty;

        return new SummaryBlockModel(summary);
    }

    public static DiagramBlockModel ToDiagramBlock(
        LearnKitArticleBlockDetails block)
    {
        var json = LearnKitBlockJsonHelper.Parse(block.ContentJson);

        var diagram = LearnKitBlockJsonHelper.GetString(json, "diagram");
        var title = LearnKitBlockJsonHelper.GetString(json, "title");
        var showTitleBar = LearnKitBlockJsonHelper.GetBoolean(json, "showDiagramTitleBar", true);
        var diagramType = LearnKitBlockJsonHelper.GetString(json, "diagramType");

        return new DiagramBlockModel(
            diagram ?? string.Empty,
            title,
            showTitleBar,
            diagramType ?? "PlantUml");
    }

    public static CodeBlockModel ToCodeBlock(
        LearnKitArticleBlockDetails block)
    {
        var json = LearnKitBlockJsonHelper.Parse(block.ContentJson);

        return new CodeBlockModel(
            LearnKitBlockJsonHelper.GetString(json, "code") ?? string.Empty,
            LearnKitBlockJsonHelper.GetString(json, "language"),
            LearnKitBlockJsonHelper.GetString(json, "fileName"),
            LearnKitBlockJsonHelper.GetString(json, "codeTitle"),
            LearnKitBlockJsonHelper.GetBoolean(json, "showCodeTitleBar", true));
    }

    public static TableBlockModel ToTableBlock(
        LearnKitArticleBlockDetails block)
    {
        var json = LearnKitBlockJsonHelper.Parse(block.ContentJson);
        var rows = new List<IReadOnlyList<TableCellModel>>();

        if (json.TryGetProperty("rows", out var rowsJson) && rowsJson.ValueKind == JsonValueKind.Array)
        {
            foreach (var rowJson in rowsJson.EnumerateArray())
            {
                var cells = new List<TableCellModel>();

                if (rowJson.ValueKind != JsonValueKind.Array)
                {
                    rows.Add(cells);
                    continue;
                }

                foreach (var cellJson in rowJson.EnumerateArray())
                {
                    cells.Add(new TableCellModel(
                        LearnKitBlockJsonHelper.GetString(cellJson, "text") ?? string.Empty,
                        LearnKitBlockJsonHelper.GetString(cellJson, "emoji"),
                        LearnKitBlockJsonHelper.GetString(cellJson, "imageUrl"),
                        LearnKitBlockJsonHelper.GetString(cellJson, "imageAlt"),
                        LearnKitBlockJsonHelper.GetString(cellJson, "horizontalAlignment"),
                        LearnKitBlockJsonHelper.GetString(cellJson, "verticalAlignment")));
                }

                rows.Add(cells);
            }
        }

        return new TableBlockModel(
            LearnKitBlockJsonHelper.GetBoolean(json, "hasHeaderRow"),
            LearnKitBlockJsonHelper.GetBoolean(json, "hasHeaderColumn"),
            LearnKitBlockJsonHelper.GetBoolean(json, "autoNumberRows"),
            LearnKitBlockJsonHelper.GetString(json, "tableStyle") ?? "dense-engineering",
            LearnKitBlockJsonHelper.GetString(json, "defaultHorizontalAlignment") ?? "left",
            LearnKitBlockJsonHelper.GetString(json, "defaultVerticalAlignment") ?? "middle",
            rows);
    }

    public static CalloutBlockModel ToCalloutBlock(
        LearnKitArticleBlockDetails block)
    {
        var json = LearnKitBlockJsonHelper.Parse(block.ContentJson);

        return new CalloutBlockModel(
            LearnKitBlockJsonHelper.GetString(json, "kind") ?? "note",
            LearnKitBlockJsonHelper.GetString(json, "text") ?? string.Empty);
    }
}
