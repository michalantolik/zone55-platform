using System.Collections.ObjectModel;

namespace LearnKit.Infrastructure.Seed.Content.Models;

/// <summary>
/// Represents one learning path stored in the content seed.
/// </summary>
public sealed class LearningPathSeed
{
    /// <summary>
    /// Stable unique identifier of the learning path.
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
    /// Determines the display order.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Learning zones belonging to this path.
    /// </summary>
    public Collection<LearningZoneSeed> Zones { get; init; } = [];
}
