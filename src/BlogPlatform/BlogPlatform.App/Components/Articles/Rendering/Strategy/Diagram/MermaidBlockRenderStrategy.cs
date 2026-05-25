using BlogPlatform.App.Components.Articles.Rendering.Context;
using BlogPlatform.Contracts.Posts;
using Microsoft.AspNetCore.Components;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Diagram;

public sealed class MermaidBlockRenderStrategy : IArticleBlockRenderStrategy
{
    public ArticleBlockType Type => ArticleBlockType.Mermaid;

    public RenderFragment Render(
        ArticleBlockDto block,
        ArticleBlockRenderContext context) => builder =>
        {
            var sequence = 0;

            builder.OpenElement(sequence++, "figure");
            builder.AddAttribute(
                sequence++,
                "class",
                ArticleBlockDiagramRenderer.GetDiagramBlockCssClass(block, "mermaid-block"));

            if (block.ShowDiagramTitleBar)
            {
                builder.OpenElement(sequence++, "figcaption");
                builder.AddContent(sequence++, ArticleBlockDiagramRenderer.GetMermaidDiagramTitle(block));
                builder.CloseElement();
            }

            builder.OpenElement(sequence++, "pre");
            builder.AddAttribute(sequence++, "class", "mermaid");
            builder.AddAttribute(sequence++, "data-mermaid-source", block.Diagram);
            builder.AddContent(sequence++, block.Diagram);
            builder.CloseElement();

            builder.CloseElement();
        };
}
