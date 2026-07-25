using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.DomainModel;
using Zone55.Portal.Components.Articles.LearnKitRendering.Blocks;
using Zone55.Portal.Models.LearnKit.Articles;

namespace Zone55.Portal.Components.Articles.LearnKitRendering.Provider;

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
