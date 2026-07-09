using BlogPlatform.App.Components.Articles.LearnKitRendering.Blocks;
using BlogPlatform.App.Models.LearnKit;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Strategy;

public sealed class LearnKitBlockComponentTypeProvider
{
    public Type? GetComponentType(LearnKitArticleBlockDetails block)
    {
        return block.Type switch
        {
            LearnKitBlockTypes.Markdown => typeof(MarkdownBlock),
            LearnKitBlockTypes.Summary => typeof(SummaryBlock),
            LearnKitBlockTypes.Diagram => typeof(DiagramBlock),
            _ => null
        };
    }
}
