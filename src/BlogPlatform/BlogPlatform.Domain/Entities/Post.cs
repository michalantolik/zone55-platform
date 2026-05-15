using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities;

public sealed class Post
{
    public string Slug { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string CategorySlug { get; init; } = string.Empty;

    public string Level { get; init; } = string.Empty;

    public string Focus { get; init; } = string.Empty;

    public string? DotnetZone { get; init; }

    public string? DotnetZoneStep { get; init; }

    public IReadOnlyCollection<string> Tags { get; init; } = [];

    public DateTimeOffset? PublishedDate { get; init; }

    public string BodyHtml { get; init; } = string.Empty;

    public PostStatus Status { get; init; } = PostStatus.Published;
}
