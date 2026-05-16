namespace BlogPlatform.Contracts.Posts;

public sealed record PostListItemDto(
    string Slug,
    string Title,
    string Summary,
    string Level,
    string Focus,
    string? DotnetZone,
    string? DotnetZoneStep,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset? PublishedDate);
