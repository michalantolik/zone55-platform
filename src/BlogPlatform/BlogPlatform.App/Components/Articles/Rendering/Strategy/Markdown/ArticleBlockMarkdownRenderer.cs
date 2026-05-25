using BlogPlatform.App.Services;
using Microsoft.AspNetCore.Components;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Markdown;

public static class ArticleBlockMarkdownRenderer
{
    public static MarkupString Render(string? markdown)
    {
        return new MarkupString(MarkdownTextRenderer.ToHtml(markdown));
    }

    public static string ToHtml(string? markdown)
    {
        return MarkdownTextRenderer.ToHtml(markdown);
    }
}
