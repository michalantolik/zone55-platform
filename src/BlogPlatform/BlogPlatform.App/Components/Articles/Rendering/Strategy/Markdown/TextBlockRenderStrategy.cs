using BlogPlatform.App.Components.Articles.Rendering.Context;
using BlogPlatform.Contracts.Posts;
using Microsoft.AspNetCore.Components;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Markdown;

public sealed class TextBlockRenderStrategy : IArticleBlockRenderStrategy
{
    public ArticleBlockType Type => ArticleBlockType.Text;

    public RenderFragment Render(
        ArticleBlockDto block,
        ArticleBlockRenderContext context) => builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "article-markdown-block");
            builder.AddContent(2, ArticleBlockRenderContext.RenderMarkdown(block.Text));
            builder.CloseElement();
        };
}
