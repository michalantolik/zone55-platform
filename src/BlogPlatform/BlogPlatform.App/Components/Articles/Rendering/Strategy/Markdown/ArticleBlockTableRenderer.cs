using BlogPlatform.Contracts.Posts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Markdown;

public static class ArticleBlockTableRenderer
{
    public static RenderFragment Render(ArticleBlockDto block) => builder =>
    {
        var sequence = 0;
        var tableStyle = NormalizeTableStyle(block.TableOptions.TableStyle);

        builder.OpenElement(sequence++, "div");
        builder.AddAttribute(
            sequence++,
            "class",
            $"article-table-wrap article-table-style-{tableStyle}");

        builder.OpenElement(sequence++, "table");
        builder.AddAttribute(sequence++, "class", "article-table");

        builder.OpenElement(sequence++, "tbody");

        for (var rowIndex = 0; rowIndex < block.TableRows.Count; rowIndex++)
        {
            var row = block.TableRows[rowIndex];

            builder.OpenElement(sequence++, "tr");

            for (var columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                var cell = row[columnIndex];
                var isHeader =
                    block.TableOptions.HasHeaderRow && rowIndex == 0 ||
                    block.TableOptions.HasHeaderColumn && columnIndex == 0;

                var tagName = isHeader ? "th" : "td";
                var horizontalAlignment = cell.HorizontalAlignment ?? block.TableOptions.DefaultHorizontalAlignment;
                var verticalAlignment = cell.VerticalAlignment ?? block.TableOptions.DefaultVerticalAlignment;

                builder.OpenElement(sequence++, tagName);
                builder.AddAttribute(
                    sequence++,
                    "style",
                    $"text-align:{NormalizeHorizontalAlignment(horizontalAlignment)};vertical-align:{NormalizeVerticalAlignment(verticalAlignment)};");

                if (block.TableOptions.AutoNumberRows && columnIndex == 0 && !isHeader)
                {
                    builder.OpenElement(sequence++, "span");
                    builder.AddAttribute(sequence++, "class", "table-row-number");
                    builder.AddContent(sequence++, rowIndex.ToString());
                    builder.CloseElement();
                }

                if (!string.IsNullOrWhiteSpace(cell.Emoji))
                {
                    builder.OpenElement(sequence++, "span");
                    builder.AddAttribute(sequence++, "class", "table-cell-emoji");
                    builder.AddContent(sequence++, cell.Emoji);
                    builder.CloseElement();
                }

                if (!string.IsNullOrWhiteSpace(cell.ImageUrl))
                {
                    builder.OpenElement(sequence++, "img");
                    builder.AddAttribute(sequence++, "class", "table-cell-image");
                    builder.AddAttribute(sequence++, "src", cell.ImageUrl);
                    builder.AddAttribute(sequence++, "alt", cell.ImageAlt ?? string.Empty);
                    builder.CloseElement();
                }

                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "table-cell-text table-cell-markdown");
                builder.AddMarkupContent(sequence++, ArticleBlockMarkdownRenderer.ToHtml(cell.Text));
                builder.CloseElement();

                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();
    };

    private static string NormalizeTableStyle(string? value)
    {
        return value is "minimal-reference"
            ? "minimal-reference"
            : "dense-engineering";
    }

    private static string NormalizeHorizontalAlignment(string? value)
    {
        return value is "center" or "right"
            ? value
            : "left";
    }

    private static string NormalizeVerticalAlignment(string? value)
    {
        return value is "top" or "bottom"
            ? value
            : "middle";
    }
}
