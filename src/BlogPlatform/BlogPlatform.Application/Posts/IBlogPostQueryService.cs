namespace BlogPlatform.Application.Posts;

public interface IBlogPostQueryService
{
    Task<IReadOnlyCollection<PostListItem>> GetPublishedPostsAsync(
        GetPublishedPostsQuery query,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PostListItem>> GetPublishedPostsForStepAsync(
        GetPublishedPostsForStepQuery query,
        CancellationToken cancellationToken);

    Task<PostDetails?> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken);
}
