using BlogPlatform.App.Components.Articles.Rendering.Context;
using BlogPlatform.Contracts.Posts;
using Microsoft.AspNetCore.Components;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Heading;

public sealed class HeadingBlockRenderStrategy : IArticleBlockRenderStrategy
{
    public ArticleBlockType Type => ArticleBlockType.Heading;

    public RenderFragment Render(
        ArticleBlockDto block,
        ArticleBlockRenderContext context) => builder =>
        {
            var tagName = block.Level switch
            {
                3 => "h3",
                4 => "h4",
                _ => "h2"
            };

            builder.OpenElement(0, tagName);
            builder.AddContent(1, block.Text);
            builder.CloseElement();
        };
}
