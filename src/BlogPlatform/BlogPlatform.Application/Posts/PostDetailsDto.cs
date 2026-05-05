namespace BlogPlatform.Application.Posts;

public sealed record PostDetailsDto(
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
