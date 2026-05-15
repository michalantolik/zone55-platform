namespace BlogPlatform.Application.Posts;

public sealed record CategorySummary(
    string Slug,
    string Name,
    int Count);
