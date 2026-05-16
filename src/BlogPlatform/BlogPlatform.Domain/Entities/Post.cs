using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.ValueObjects;

namespace BlogPlatform.Domain.Entities;

public sealed class Post
{
    private Post(
        string slug,
        string title,
        string summary,
        string category,
        string categorySlug,
        string level,
        string focus,
        string? dotnetZone,
        string? dotnetZoneStep,
        IReadOnlyCollection<string> tags,
        DateTimeOffset? publishedDate,
        string bodyHtml,
        PostStatus status)
    {
        Slug = slug;
        Title = title;
        Summary = summary;
        Category = category;
        CategorySlug = categorySlug;
        Level = level;
        Focus = focus;
        DotnetZone = dotnetZone;
        DotnetZoneStep = dotnetZoneStep;
        Tags = tags;
        PublishedDate = publishedDate;
        BodyHtml = bodyHtml;
        Status = status;
    }

    public string Slug { get; }

    public string Title { get; }

    public string Summary { get; }

    public string Category { get; }

    public string CategorySlug { get; }

    public string Level { get; }

    public string Focus { get; }

    public string? DotnetZone { get; }

    public string? DotnetZoneStep { get; }

    public IReadOnlyCollection<string> Tags { get; }

    public DateTimeOffset? PublishedDate { get; }

    public string BodyHtml { get; }

    public PostStatus Status { get; }

    public bool IsPublished => Status == PostStatus.Published;

    public bool MatchesCategorySlug(string? categorySlug)
    {
        if (string.IsNullOrWhiteSpace(categorySlug))
        {
            return true;
        }

        return string.Equals(
            CategorySlug,
            categorySlug.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    public bool MatchesDotnetStep(string? dotnetZone, string? dotnetZoneStep)
    {
        if (string.IsNullOrWhiteSpace(dotnetZone) ||
            string.IsNullOrWhiteSpace(dotnetZoneStep))
        {
            return false;
        }

        return string.Equals(DotnetZone, dotnetZone.Trim(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(DotnetZoneStep, dotnetZoneStep.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public int LevelSortOrder
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Level))
            {
                return 50;
            }

            if (Level.Contains("beginner", StringComparison.OrdinalIgnoreCase) ||
                Level.Contains("basic", StringComparison.OrdinalIgnoreCase) ||
                Level.Contains("fundamental", StringComparison.OrdinalIgnoreCase))
            {
                return 10;
            }

            if (Level.Contains("intermediate", StringComparison.OrdinalIgnoreCase))
            {
                return 20;
            }

            if (Level.Contains("advanced", StringComparison.OrdinalIgnoreCase))
            {
                return 30;
            }

            return 50;
        }
    }

    public static Post Create(
        string? slug,
        string? title,
        string? summary,
        string? category,
        string? categorySlug,
        string? level,
        string? focus,
        string? dotnetZone,
        string? dotnetZoneStep,
        IReadOnlyCollection<string>? tags,
        DateTimeOffset? publishedDate,
        string? bodyHtml,
        PostStatus status)
    {
        return new Post(
            BlogPlatform.Domain.ValueObjects.Slug.Create(slug).Value,
            NormalizeRequired(title),
            NormalizeOptionalAsEmpty(summary),
            NormalizeOptionalAsEmpty(category),
            NormalizeOptionalAsEmpty(categorySlug),
            NormalizeOptionalAsEmpty(level),
            NormalizeOptionalAsEmpty(focus),
            NormalizeOptional(dotnetZone),
            NormalizeOptional(dotnetZoneStep),
            NormalizeTags(tags),
            publishedDate,
            NormalizeOptionalAsEmpty(bodyHtml),
            status);
    }

    public static Post CreatePublished(
        string? slug,
        string? title,
        string? summary,
        string? category,
        string? categorySlug,
        string? level,
        string? focus,
        string? dotnetZone,
        string? dotnetZoneStep,
        IReadOnlyCollection<string>? tags,
        DateTimeOffset? publishedDate,
        string? bodyHtml)
    {
        return Create(
            slug,
            title,
            summary,
            category,
            categorySlug,
            level,
            focus,
            dotnetZone,
            dotnetZoneStep,
            tags,
            publishedDate,
            bodyHtml,
            PostStatus.Published);
    }

    private static string NormalizeRequired(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Required post value cannot be empty.");
        }

        return value.Trim();
    }

    private static string NormalizeOptionalAsEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static IReadOnlyCollection<string> NormalizeTags(
        IReadOnlyCollection<string>? tags)
    {
        return tags?
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];
    }
}
