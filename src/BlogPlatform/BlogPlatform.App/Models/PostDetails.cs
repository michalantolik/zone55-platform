namespace BlogPlatform.App.Models;

public sealed record PostDetails(
    string Slug,
    string Title,
    string Summary,
    string Level,
    string Focus,
    string? Zone,
    string? Step,
    int Order,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset? PublishedDate,
    string BodyHtml);
