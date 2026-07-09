using BlogPlatform.App.Components.Articles.LearnKitRendering.Helpers;
using BlogPlatform.App.Components.Articles.LearnKitRendering.Models;
using BlogPlatform.App.Models.LearnKit;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization;

public static class LearnKitBlockDeserializer
{
    public static MarkdownBlockModel ToMarkdownBlock(
        LearnKitArticleBlockDetails block)
    {
        var json = LearnKitBlockJsonHelper.Parse(block.ContentJson);

        var markdown =
            json.GetProperty("markdown")
                .GetString();

        return new MarkdownBlockModel(markdown ?? string.Empty);
    }

    public static SummaryBlockModel ToSummaryBlock(
        LearnKitArticleBlockDetails block)
    {
        var json = LearnKitBlockJsonHelper.Parse(block.ContentJson);

        var summary =
            json.GetProperty("summary")
                .GetString();

        return new SummaryBlockModel(summary ?? string.Empty);
    }

    public static DiagramBlockModel ToDiagramBlock(
        LearnKitArticleBlockDetails block)
    {
        var json = LearnKitBlockJsonHelper.Parse(block.ContentJson);

        var diagram = json.GetProperty("diagram").GetString();
        var title = json.GetProperty("title").GetString();
        var showTitleBar = json.GetProperty("showDiagramTitleBar").GetBoolean();
        var diagramType = json.GetProperty("diagramType").GetString();

        return new DiagramBlockModel(
            diagram ?? string.Empty,
            title,
            showTitleBar,
            diagramType ?? string.Empty);
    }
}
