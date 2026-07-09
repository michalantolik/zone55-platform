using System.Collections.ObjectModel;

namespace LearnKit.Infrastructure.Seed.Content.Models;

/// <summary>
/// Represents one learning step stored in the content seed.
/// </summary>
public sealed class LearningStepSeed
{
    /// <summary>
    /// Stable unique identifier of the learning step.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Display title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Optional short description.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Determines the display order within the learning zone.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Articles belonging to this learning step.
    /// </summary>
    public Collection<ArticleSeed> Articles { get; init; } = [];
}
