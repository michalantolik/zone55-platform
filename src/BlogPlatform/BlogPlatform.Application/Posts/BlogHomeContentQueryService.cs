namespace BlogPlatform.Application.Posts;

public sealed class BlogHomeContentQueryService : IBlogHomeContentQueryService
{
    private readonly IBlogPostQueryService _posts;

    public BlogHomeContentQueryService(IBlogPostQueryService posts)
    {
        _posts = posts;
    }

    public async Task<BlogHomeContent> GetHomeContentAsync(
        string? categorySlug,
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPublishedPostsAsync(
            new GetPublishedPostsQuery(categorySlug),
            cancellationToken);

        return new BlogHomeContent(posts);
    }
}
