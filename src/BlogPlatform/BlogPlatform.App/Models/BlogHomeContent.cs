namespace BlogPlatform.App.Models;

public sealed record BlogHomeContent(
    IReadOnlyCollection<PostListItem> Posts);
