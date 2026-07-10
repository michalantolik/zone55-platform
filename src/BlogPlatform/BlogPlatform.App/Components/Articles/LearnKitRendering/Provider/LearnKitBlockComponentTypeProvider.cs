using BlogPlatform.App.Components.Articles.LearnKitRendering.Blocks;
using BlogPlatform.App.Models.LearnKit.Articles;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Provider;

public sealed class LearnKitBlockComponentTypeProvider
{
    public Type? GetComponentType(LearnKitArticleBlockDetails block)
    {
        return block.Type switch
        {
            LearnKitBlockTypes.Markdown => typeof(MarkdownBlock),
            LearnKitBlockTypes.Summary => typeof(SummaryBlock),
            LearnKitBlockTypes.Diagram => typeof(DiagramBlock),
            LearnKitBlockTypes.Code => typeof(CodeBlock),
            LearnKitBlockTypes.Table => typeof(TableBlock),
            LearnKitBlockTypes.Callout => typeof(CalloutBlock),
            _ => null
        };
    }
}
