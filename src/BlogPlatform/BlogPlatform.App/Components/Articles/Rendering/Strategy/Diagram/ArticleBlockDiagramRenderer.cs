using BlogPlatform.App.Components.Articles.Rendering.Context;
using BlogPlatform.App.Components.Articles.Shared;
using BlogPlatform.Contracts.Posts;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Diagram;

public static class ArticleBlockDiagramRenderer
{
    public static string GetDiagramBlockCssClass(
        ArticleBlockDto block,
        string diagramTypeClass)
    {
        return DiagramRenderer.GetDiagramBlockCssClass(
            block.ShowDiagramTitleBar,
            diagramTypeClass);
    }

    public static string GetMermaidDiagramTitle(ArticleBlockDto block)
    {
        return DiagramRenderer.GetMermaidDiagramTitle(
            block.Diagram,
            block.DiagramTitle);
    }

    public static string GetPlantUmlDiagramTitle(ArticleBlockDto block)
    {
        return DiagramRenderer.GetPlantUmlDiagramTitle(
            block.Diagram,
            block.DiagramTitle);
    }

    public static string CreatePlantUmlUrl(
        string? diagram,
        PlantUmlThemeValues theme)
    {
        return DiagramRenderer.CreatePlantUmlUrl(diagram, theme);
    }
}
