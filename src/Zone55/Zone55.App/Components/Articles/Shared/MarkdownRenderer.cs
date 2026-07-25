using Zone55.App.Services;
using Microsoft.AspNetCore.Components;

namespace Zone55.App.Components.Articles.Shared;

public static class MarkdownRenderer
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
