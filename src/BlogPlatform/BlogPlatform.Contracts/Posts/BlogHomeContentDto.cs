namespace BlogPlatform.Contracts.Posts;

public sealed record BlogHomeContentDto(
    IReadOnlyCollection<PostListItemDto> Posts);
