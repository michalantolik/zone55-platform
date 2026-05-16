using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Posts;

internal sealed class BlogHomeContentQueryService : IBlogHomeContentQueryService
{
    private readonly IBlogPostRepository _posts;

    public BlogHomeContentQueryService(IBlogPostRepository posts)
    {
        _posts = posts;
    }

    public async Task<BlogHomeContent> GetHomeContentAsync(
        string? categorySlug,
        CancellationToken cancellationToken)
    {
        var publishedPosts = await GetPublishedPostsAsync(cancellationToken);

        var categories = publishedPosts
            .GroupBy(post => new { post.CategorySlug, post.Category })
            .Where(group => !string.IsNullOrWhiteSpace(group.Key.CategorySlug))
            .Select(group => new CategorySummary(
                group.Key.CategorySlug,
                group.Key.Category,
                group.Count()))
            .OrderBy(category => category.Name)
            .ToList();

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            publishedPosts = publishedPosts
                .Where(post => string.Equals(
                    post.CategorySlug,
                    categorySlug,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var posts = publishedPosts
            .OrderByDescending(post => post.PublishedDate)
            .Select(ToListItem)
            .ToList();

        return new BlogHomeContent(categories, posts);
    }

    private async Task<IReadOnlyCollection<Post>> GetPublishedPostsAsync(
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPostsAsync(cancellationToken);

        return posts
            .Where(post => post.IsPublished)
            .ToList();
    }

    private static PostListItem ToListItem(Post post)
    {
        return new PostListItem(
            post.Slug,
            post.Title,
            post.Summary,
            post.Category,
            post.CategorySlug,
            post.Level,
            post.Focus,
            post.DotnetZone,
            post.DotnetZoneStep,
            post.Tags,
            post.PublishedDate);
    }
}
