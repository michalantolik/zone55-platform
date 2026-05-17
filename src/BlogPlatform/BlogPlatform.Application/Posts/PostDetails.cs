namespace BlogPlatform.Application.Posts;

public sealed record PostDetails(
    string Slug,
    string Title,
    string Summary,
    string Level,
    string Focus,
    string? DotnetZone,
    string? DotnetZoneStep,
    int Order,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset? PublishedDate,
    string BodyHtml);
