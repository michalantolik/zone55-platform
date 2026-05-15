namespace BlogPlatform.Application.Posts;

public sealed record BlogHomeContentDto(
    IReadOnlyCollection<CategoryDto> Categories,
    IReadOnlyCollection<PostListItemDto> Posts);
