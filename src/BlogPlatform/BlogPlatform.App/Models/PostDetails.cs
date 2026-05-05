namespace BlogPlatform.App.Models;

public sealed record PostDetails(
    string Slug,
    string Title,
    string Summary,
    string Category,
    string CategorySlug,
    string Level,
    string Focus,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset? PublishedDate,
    string BodyHtml);
