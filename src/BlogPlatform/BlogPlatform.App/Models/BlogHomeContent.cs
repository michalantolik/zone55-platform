namespace BlogPlatform.App.Models;

public sealed record BlogHomeContent(
    IReadOnlyCollection<CategoryItem> Categories,
    IReadOnlyCollection<PostListItem> Posts);
