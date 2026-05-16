namespace BlogPlatform.Application.Posts;

public sealed record BlogHomeContent(
    IReadOnlyCollection<PostListItem> Posts);
