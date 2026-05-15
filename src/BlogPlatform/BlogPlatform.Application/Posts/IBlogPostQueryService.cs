namespace BlogPlatform.Application.Posts;

public interface IBlogPostQueryService
{
    Task<IReadOnlyCollection<PostListItem>> GetPublishedPostsAsync(
        string? categorySlug,
        CancellationToken cancellationToken);

    Task<PostDetails?> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CategorySummary>> GetCategoriesAsync(
        CancellationToken cancellationToken);
}
