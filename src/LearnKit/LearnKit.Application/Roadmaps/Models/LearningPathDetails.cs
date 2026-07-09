namespace LearnKit.Application.Roadmaps.Models;

/// <summary>
/// Details of a learning path with its zones.
/// </summary>
public sealed class LearningPathDetails
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public IReadOnlyCollection<LearningZoneDetails> Zones { get; init; } = [];
}
