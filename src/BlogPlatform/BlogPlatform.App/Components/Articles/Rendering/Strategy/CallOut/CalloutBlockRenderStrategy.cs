using BlogPlatform.App.Components.Articles.Rendering.Context;
using BlogPlatform.Contracts.Posts;
using Microsoft.AspNetCore.Components;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.CallOut;

public sealed class CalloutBlockRenderStrategy : IArticleBlockRenderStrategy
{
    public ArticleBlockType Type => ArticleBlockType.Callout;

    public RenderFragment Render(
        ArticleBlockDto block,
        ArticleBlockRenderContext context) => builder =>
        {
            var sequence = 0;

            builder.OpenElement(sequence++, "aside");
            builder.AddAttribute(sequence++, "class", $"callout callout-{block.Kind}");

            builder.OpenElement(sequence++, "strong");
            builder.AddContent(sequence++, block.Kind);
            builder.CloseElement();

            builder.OpenElement(sequence++, "p");
            builder.AddContent(sequence++, block.Text);
            builder.CloseElement();

            builder.CloseElement();
        };
}
