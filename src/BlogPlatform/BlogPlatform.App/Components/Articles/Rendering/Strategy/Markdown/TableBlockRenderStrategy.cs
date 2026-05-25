using BlogPlatform.App.Components.Articles.Rendering.Context;
using BlogPlatform.Contracts.Posts;
using Microsoft.AspNetCore.Components;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Markdown;

public sealed class TableBlockRenderStrategy : IArticleBlockRenderStrategy
{
    public ArticleBlockType Type => ArticleBlockType.Table;

    public RenderFragment Render(
        ArticleBlockDto block,
        ArticleBlockRenderContext context)
    {
        return ArticleBlockTableRenderer.Render(block);
    }
}
