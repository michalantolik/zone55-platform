using BlogPlatform.Contracts.Posts;

namespace BlogPlatform.Application.Posts;

public interface IBlogPostQueryService
{
    Task<IReadOnlyCollection<PostListItemDto>> GetPublishedPostsAsync(
        string? categorySlug,
        CancellationToken cancellationToken);

    Task<PostDetailsDto?> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken);
}
