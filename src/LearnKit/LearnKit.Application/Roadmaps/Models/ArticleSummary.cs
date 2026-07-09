namespace LearnKit.Application.Roadmaps.Models;

/// <summary>
/// Short article information used on the learning roadmap.
/// </summary>
public sealed class ArticleSummary
{
    public required string Slug { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required string Status { get; init; }

    public int SortOrder { get; init; }
}
