namespace LearnKit.Application.Roadmaps.Models;

/// <summary>
/// Details of a learning step with its articles.
/// </summary>
public sealed class LearningStepDetails
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public int SortOrder { get; init; }

    public IReadOnlyCollection<ArticleSummary> Articles { get; init; } = [];
}
