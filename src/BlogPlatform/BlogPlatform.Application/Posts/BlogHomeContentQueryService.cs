namespace BlogPlatform.Application.Posts;

public sealed class BlogHomeContentQueryService : IBlogHomeContentQueryService
{
    private readonly IBlogPostQueryService _posts;

    public BlogHomeContentQueryService(IBlogPostQueryService posts)
    {
        _posts = posts;
    }

    public async Task<BlogHomeContent> GetHomeContentAsync(
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPublishedPostsAsync(
            GetPublishedPostsQuery.All,
            cancellationToken);

        return new BlogHomeContent(posts);
    }
}
