using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BlogPlatform.App.Services;

public static partial class MarkdownTextRenderer
{
    public static string ToHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var html = new StringBuilder();
        var paragraph = new List<string>();
        var inUnorderedList = false;
        var inOrderedList = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            var trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                FlushParagraph(html, paragraph);
                CloseLists(html, ref inUnorderedList, ref inOrderedList);
                continue;
            }

            if (trimmed.StartsWith("### ", StringComparison.Ordinal))
            {
                FlushParagraph(html, paragraph);
                CloseLists(html, ref inUnorderedList, ref inOrderedList);
                html.Append("<h3>").Append(RenderInline(trimmed[4..])).Append("</h3>");
                continue;
            }

            if (trimmed.StartsWith("## ", StringComparison.Ordinal))
            {
                FlushParagraph(html, paragraph);
                CloseLists(html, ref inUnorderedList, ref inOrderedList);
                html.Append("<h2>").Append(RenderInline(trimmed[3..])).Append("</h2>");
                continue;
            }

            if (trimmed.StartsWith("# ", StringComparison.Ordinal))
            {
                FlushParagraph(html, paragraph);
                CloseLists(html, ref inUnorderedList, ref inOrderedList);
                html.Append("<h2>").Append(RenderInline(trimmed[2..])).Append("</h2>");
                continue;
            }

            if (trimmed.StartsWith("> ", StringComparison.Ordinal))
            {
                FlushParagraph(html, paragraph);
                CloseLists(html, ref inUnorderedList, ref inOrderedList);
                html.Append("<blockquote>")
                    .Append(RenderInline(trimmed[2..]))
                    .Append("</blockquote>");
                continue;
            }

            if (trimmed.StartsWith("- ", StringComparison.Ordinal) ||
                trimmed.StartsWith("* ", StringComparison.Ordinal))
            {
                FlushParagraph(html, paragraph);

                if (inOrderedList)
                {
                    html.Append("</ol>");
                    inOrderedList = false;
                }

                if (!inUnorderedList)
                {
                    html.Append("<ul>");
                    inUnorderedList = true;
                }

                html.Append("<li>").Append(RenderInline(trimmed[2..])).Append("</li>");
                continue;
            }

            var orderedMatch = OrderedListRegex().Match(trimmed);

            if (orderedMatch.Success)
            {
                FlushParagraph(html, paragraph);

                if (inUnorderedList)
                {
                    html.Append("</ul>");
                    inUnorderedList = false;
                }

                if (!inOrderedList)
                {
                    html.Append("<ol>");
                    inOrderedList = true;
                }

                html.Append("<li>")
                    .Append(RenderInline(orderedMatch.Groups["text"].Value))
                    .Append("</li>");

                continue;
            }

            CloseLists(html, ref inUnorderedList, ref inOrderedList);
            paragraph.Add(trimmed);
        }

        FlushParagraph(html, paragraph);
        CloseLists(html, ref inUnorderedList, ref inOrderedList);

        return html.ToString();
    }

    private static void FlushParagraph(StringBuilder html, List<string> paragraph)
    {
        if (paragraph.Count == 0)
        {
            return;
        }

        html.Append("<p>")
            .Append(RenderInline(string.Join("<br />", paragraph)))
            .Append("</p>");

        paragraph.Clear();
    }

    private static void CloseLists(
        StringBuilder html,
        ref bool inUnorderedList,
        ref bool inOrderedList)
    {
        if (inUnorderedList)
        {
            html.Append("</ul>");
            inUnorderedList = false;
        }

        if (inOrderedList)
        {
            html.Append("</ol>");
            inOrderedList = false;
        }
    }

    private static string RenderInline(string value)
    {
        var encoded = WebUtility.HtmlEncode(value);

        encoded = LinkRegex().Replace(
            encoded,
            match =>
            {
                var text = match.Groups["text"].Value;
                var url = match.Groups["url"].Value;

                if (!IsSafeUrl(url))
                {
                    return text;
                }

                return $"""<a href="{url}" target="_blank" rel="noopener noreferrer">{text}</a>""";
            });

        encoded = InlineCodeRegex().Replace(encoded, "<code>$1</code>");
        encoded = BoldRegex().Replace(encoded, "<strong>$1</strong>");
        encoded = ItalicRegex().Replace(encoded, "<em>$1</em>");

        return encoded;
    }

    private static bool IsSafeUrl(string url)
    {
        return url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("/", StringComparison.Ordinal);
    }

    [GeneratedRegex(@"^\d+\.\s+(?<text>.+)$")]
    private static partial Regex OrderedListRegex();

    [GeneratedRegex(@"\[(?<text>[^\]]+)\]\((?<url>[^)]+)\)")]
    private static partial Regex LinkRegex();

    [GeneratedRegex(@"`([^`]+)`")]
    private static partial Regex InlineCodeRegex();

    [GeneratedRegex(@"\*\*([^*]+)\*\*")]
    private static partial Regex BoldRegex();

    [GeneratedRegex(@"(?<!\*)\*([^*]+)\*(?!\*)")]
    private static partial Regex ItalicRegex();
}
