using BlogPlatform.App.Components.Articles.Rendering.Context;
using BlogPlatform.Contracts.Posts;
using Microsoft.AspNetCore.Components;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Diagram;

public sealed class PlantUmlBlockRenderStrategy : IArticleBlockRenderStrategy
{
    public ArticleBlockType Type => ArticleBlockType.PlantUml;

    public RenderFragment Render(
        ArticleBlockDto block,
        ArticleBlockRenderContext context) => builder =>
        {
            var sequence = 0;

            builder.OpenElement(sequence++, "figure");
            builder.AddAttribute(
                sequence++,
                "class",
                ArticleBlockDiagramRenderer.GetDiagramBlockCssClass(block, "plantuml-block"));

            if (block.ShowDiagramTitleBar)
            {
                builder.OpenElement(sequence++, "figcaption");
                builder.AddContent(sequence++, ArticleBlockDiagramRenderer.GetPlantUmlDiagramTitle(block));
                builder.CloseElement();
            }

            builder.OpenElement(sequence++, "img");
            builder.AddAttribute(
                sequence++,
                "src",
                ArticleBlockDiagramRenderer.CreatePlantUmlUrl(block.Diagram, context.PlantUmlTheme));
            builder.AddAttribute(sequence++, "alt", "PlantUML diagram");
            builder.CloseElement();

            builder.CloseElement();
        };
}
