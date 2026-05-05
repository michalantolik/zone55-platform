namespace BlogPlatform.App.Models;

public sealed record CategoryItem(
    string Slug,
    string Name,
    int Count);
