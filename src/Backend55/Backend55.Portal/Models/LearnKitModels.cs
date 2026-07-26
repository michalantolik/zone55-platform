namespace Backend55.Portal.Models;

public sealed class LearningPathDetails
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public string Summary { get; init; } = string.Empty;

    public int SortOrder { get; init; }

    public IReadOnlyCollection<LearningZoneDetails> Zones { get; init; } = [];
}

public sealed class LearningZoneDetails
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public string Summary { get; init; } = string.Empty;

    public int SortOrder { get; init; }

    public IReadOnlyCollection<LearningStepDetails> Steps { get; init; } = [];
}

public sealed class LearningStepDetails
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public string Summary { get; init; } = string.Empty;

    public int SortOrder { get; init; }

    public IReadOnlyCollection<ArticleSummary> Articles { get; init; } = [];
}

public sealed class ArticleSummary
{
    public required string Slug { get; init; }

    public required string Title { get; init; }

    public string Summary { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public int SortOrder { get; init; }
}

public sealed class ArticleDetails
{
    public required string Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public string? Summary { get; init; }

    public string? Status { get; init; }

    public string LanguageCode { get; init; } = "en";

    public bool IsFallback { get; init; }

    public IReadOnlyList<ArticleBlockDetails> Blocks { get; init; } = [];
}

public sealed class ArticleBlockDetails
{
    public required string Id { get; init; }

    public required string Type { get; init; }

    public int SortOrder { get; init; }

    public required string ContentJson { get; init; }
}
