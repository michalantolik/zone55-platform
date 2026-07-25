using Zone55.Portal.Services;
using Microsoft.AspNetCore.Components;

namespace Zone55.Portal.Components.Articles.Shared;

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
