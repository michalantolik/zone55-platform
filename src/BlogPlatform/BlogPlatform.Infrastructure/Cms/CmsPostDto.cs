namespace BlogPlatform.Infrastructure.Cms;

internal sealed record CmsPostDto(
    Guid Key,
    string? Slug,
    string? Title,
    string? Summary,
    string? Level,
    string? Focus,
    string? DotnetZone,
    string? DotnetZoneStep,
    int Order,
    IReadOnlyCollection<string>? Tags,
    DateTimeOffset? PublishedDate,
    string? BodyHtml,
    DateTime UpdatedUtc);
