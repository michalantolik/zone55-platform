using LearnKit.Domain.Articles;
using Zone55.App.Components.Articles.LearnKitRendering.Blocks;
using Zone55.App.Models.LearnKit.Articles;

namespace Zone55.App.Components.Articles.LearnKitRendering.Provider;

public sealed class LearnKitBlockComponentTypeProvider
{
    public Type? GetComponentType(LearnKitArticleBlockDetails block)
    {
        if (!ArticleBlockTypeResolver.TryResolve(block.Type, out var blockType))
        {
            return null;
        }

        return blockType switch
        {
            ArticleBlockType.Markdown => typeof(MarkdownBlock),
            ArticleBlockType.Summary => typeof(SummaryBlock),
            ArticleBlockType.Diagram => typeof(DiagramBlock),
            ArticleBlockType.Code => typeof(CodeBlock),
            ArticleBlockType.Table => typeof(TableBlock),
            ArticleBlockType.Callout => typeof(CalloutBlock),
            _ => null
        };
    }
}
