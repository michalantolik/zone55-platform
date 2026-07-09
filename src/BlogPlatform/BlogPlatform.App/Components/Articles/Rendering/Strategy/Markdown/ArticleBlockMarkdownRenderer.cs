using BlogPlatform.App.Components.Articles.Shared;
using Microsoft.AspNetCore.Components;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Markdown;

public static class ArticleBlockMarkdownRenderer
{
    public static MarkupString Render(string? markdown)
    {
        return MarkdownRenderer.Render(markdown);
    }

    public static string ToHtml(string? markdown)
    {
        return MarkdownRenderer.ToHtml(markdown);
    }
}
