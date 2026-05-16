namespace BlogPlatform.Application.Posts;

public sealed record PostListItem(
    string Slug,
    string Title,
    string Summary,
    string Level,
    string Focus,
    string? DotnetZone,
    string? DotnetZoneStep,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset? PublishedDate);
