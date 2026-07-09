using System.Collections.ObjectModel;
using LearnKit.Domain.Articles;

namespace LearnKit.Infrastructure.Seed.Content.Models;

/// <summary>
/// Represents one article stored in the content seed.
/// </summary>
internal sealed class ArticleSeed
{
    /// <summary>
    /// Stable URL-friendly article identifier.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// Display title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Optional short description.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Current publishing state.
    /// </summary>
    public ArticleStatus Status { get; init; }

    /// <summary>
    /// Determines the display order within the learning step.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Blocks belonging to this article.
    /// </summary>
    public Collection<ArticleBlockSeed> Blocks { get; init; } = [];
}
