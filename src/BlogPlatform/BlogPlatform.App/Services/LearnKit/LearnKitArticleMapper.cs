using BlogPlatform.App.Models;
using BlogPlatform.App.Models.LearnKit;
using System.Text;
using System.Text.Json;

namespace BlogPlatform.App.Services.LearnKit;

public static class LearnKitArticleMapper
{
    public static PostDetails ToPostDetails(LearnKitArticleDetails article)
    {
        return new PostDetails(
            Slug: article.Slug,
            Title: article.Title,
            Summary: article.Summary ?? string.Empty,
            Level: string.Empty,
            Focus: string.Empty,
            Zone: string.Empty,
            Step: string.Empty,
            Order: 0,
            Tags: [],
            PublishedDate: DateTimeOffset.UtcNow,
            BodyHtml: BuildBodyHtml(article));
    }

    private static string BuildBodyHtml(LearnKitArticleDetails article)
    {
        var body = new StringBuilder();

        foreach (var block in article.Blocks.OrderBy(block => block.SortOrder))
        {
            if (!string.Equals(block.Type, "Markdown", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            using var document = JsonDocument.Parse(block.ContentJson);

            if (document.RootElement.TryGetProperty("markdown", out var markdownElement))
            {
                body.AppendLine(markdownElement.GetString());
                body.AppendLine();
            }
        }

        return body.ToString();
    }
}
