namespace BlogPlatform.App.Models.LearnKit;

public sealed class LearnKitArticleDetails
{
    public required string Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public string? Summary { get; init; }

    public string? Status { get; init; }

    public required IReadOnlyList<LearnKitArticleBlockDetails> Blocks { get; init; }
}

public sealed class LearnKitArticleBlockDetails
{
    public required string Id { get; init; }

    public required string Type { get; init; }

    public int SortOrder { get; init; }

    public required string ContentJson { get; init; }
}
