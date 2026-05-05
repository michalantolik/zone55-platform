namespace BlogPlatform.Application.Posts;

public sealed record CategoryDto(
    string Slug,
    string Name,
    int Count);
