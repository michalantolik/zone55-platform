namespace BlogPlatform.App.Models.LearnKit.Roadmap;

public sealed class LearnKitLearningPathDetails
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public IReadOnlyCollection<LearnKitLearningZoneDetails> Zones { get; init; } = [];
}

public sealed class LearnKitLearningZoneDetails
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public int SortOrder { get; init; }

    public IReadOnlyCollection<LearnKitLearningStepDetails> Steps { get; init; } = [];
}

public sealed class LearnKitLearningStepDetails
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public int SortOrder { get; init; }

    public IReadOnlyCollection<LearnKitArticleSummary> Articles { get; init; } = [];
}

public sealed class LearnKitArticleSummary
{
    public required string Slug { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required string Status { get; init; }

    public int SortOrder { get; init; }
}
