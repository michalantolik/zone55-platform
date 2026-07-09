using System.Collections.ObjectModel;

namespace LearnKit.Infrastructure.Seed.Content.Models;

/// <summary>
/// Represents one learning zone stored in the content seed.
/// </summary>
internal sealed class LearningZoneSeed
{
    /// <summary>
    /// Stable unique identifier of the learning zone.
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
    /// Determines the display order within the learning path.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Learning steps belonging to this zone.
    /// </summary>
    public Collection<LearningStepSeed> Steps { get; init; } = [];
}
