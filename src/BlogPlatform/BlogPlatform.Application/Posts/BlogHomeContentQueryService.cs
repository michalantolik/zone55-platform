namespace BlogPlatform.Application.Posts;

internal sealed class BlogHomeContentQueryService : IBlogHomeContentQueryService
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
        var categories = await _posts.GetCategoriesAsync(cancellationToken);

        var posts = await _posts.GetPublishedPostsAsync(
            categorySlug,
            cancellationToken);

        return new BlogHomeContent(categories, posts);
    }
}
