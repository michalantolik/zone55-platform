using Microsoft.AspNetCore.Components.Rendering;

namespace BlogPlatform.App.Components.Articles.Shared;

public static class TableRenderer
{
    public static void RenderCellContent(
        RenderTreeBuilder builder,
        ref int sequence,
        string? text,
        string? emoji,
        string? imageUrl,
        string? imageAlt)
    {
        if (!string.IsNullOrWhiteSpace(emoji))
        {
            builder.OpenElement(sequence++, "span");
            builder.AddAttribute(sequence++, "class", "table-cell-emoji");
            builder.AddContent(sequence++, emoji);
            builder.CloseElement();
        }

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            builder.OpenElement(sequence++, "img");
            builder.AddAttribute(sequence++, "class", "table-cell-image");
            builder.AddAttribute(sequence++, "src", imageUrl);
            builder.AddAttribute(sequence++, "alt", imageAlt ?? string.Empty);
            builder.CloseElement();
        }

        builder.OpenElement(sequence++, "div");
        builder.AddAttribute(sequence++, "class", "table-cell-text table-cell-markdown");
        builder.AddMarkupContent(sequence++, MarkdownRenderer.ToHtml(text));
        builder.CloseElement();
    }

    public static string NormalizeTableStyle(string? value)
    {
        return value is "minimal-reference"
            ? "minimal-reference"
            : "dense-engineering";
    }

    public static string NormalizeHorizontalAlignment(string? value)
    {
        return value is "center" or "right"
            ? value
            : "left";
    }

    public static string NormalizeVerticalAlignment(string? value)
    {
        return value is "top" or "bottom"
            ? value
            : "middle";
    }
}
