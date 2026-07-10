namespace LearnKit.Application.Roadmaps.Public.Models;

/// <summary>
/// Details of a learning zone with its steps.
/// </summary>
public sealed class LearningZoneDetails
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public int SortOrder { get; init; }

    public IReadOnlyCollection<LearningStepDetails> Steps { get; init; } = [];
}
