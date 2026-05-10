namespace BlogPlatform.Application.Posts;

public sealed record PostListItemDto(
    string Slug,
    string Title,
    string Summary,
    string Category,
    string CategorySlug,
    string Level,
    string Focus,
    string? DotnetZone,
    string? DotnetZoneStep,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset? PublishedDate);
