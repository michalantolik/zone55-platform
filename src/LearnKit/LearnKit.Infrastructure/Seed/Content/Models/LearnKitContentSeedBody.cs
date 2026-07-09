using System.Collections.ObjectModel;

namespace LearnKit.Infrastructure.Seed.Content.Models;

/// <summary>
/// Contains all learning content stored in the seed file.
/// </summary>
public sealed class LearnKitContentSeedBody
{
    /// <summary>
    /// Learning paths included in the seed.
    /// </summary>
    public Collection<LearningPathSeed> LearningPaths { get; init; } = [];
}
