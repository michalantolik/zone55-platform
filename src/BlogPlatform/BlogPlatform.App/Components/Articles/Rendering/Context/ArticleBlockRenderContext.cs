using BlogPlatform.App.Components.Articles.Rendering.Strategy.Markdown;
using Microsoft.AspNetCore.Components;

namespace BlogPlatform.App.Components.Articles.Rendering.Context;

public sealed class ArticleBlockRenderContext
{
    public ArticleBlockRenderContext(
        PlantUmlThemeValues plantUmlTheme,
        object eventReceiver,
        Func<string?, Task> copyToClipboardAsync)
    {
        PlantUmlTheme = plantUmlTheme;
        EventReceiver = eventReceiver;
        CopyToClipboardAsync = copyToClipboardAsync;
    }

    public PlantUmlThemeValues PlantUmlTheme { get; }

    public object EventReceiver { get; }

    public Func<string?, Task> CopyToClipboardAsync { get; }

    public static MarkupString RenderMarkdown(string? markdown)
    {
        return ArticleBlockMarkdownRenderer.Render(markdown);
    }
}
