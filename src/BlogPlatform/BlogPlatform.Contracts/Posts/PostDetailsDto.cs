namespace BlogPlatform.Contracts.Posts;

public sealed record PostDetailsDto(
    string Slug,
    string Title,
    string Summary,
    string Level,
    string Focus,
    string? DotnetZone,
    string? DotnetZoneStep,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset? PublishedDate,
    string BodyHtml);
