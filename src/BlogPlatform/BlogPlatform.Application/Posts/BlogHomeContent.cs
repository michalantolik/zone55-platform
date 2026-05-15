namespace BlogPlatform.Application.Posts;

public sealed record BlogHomeContent(
    IReadOnlyCollection<CategorySummary> Categories,
    IReadOnlyCollection<PostListItem> Posts);
