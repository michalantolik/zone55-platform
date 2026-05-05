namespace BlogPlatform.Application.Posts;

public sealed record PostListItemDto(
    string Slug,
    string Title,
    string Summary,
    string Category,
    string CategorySlug,
    string Level,
    string Focus,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset? PublishedDate);
