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
        var categoriesTask = _posts.GetCategoriesAsync(cancellationToken);
        var postsTask = _posts.GetPublishedPostsAsync(categorySlug, cancellationToken);

        await Task.WhenAll(categoriesTask, postsTask);

        return new BlogHomeContent(
            await categoriesTask,
            await postsTask);
    }
}
