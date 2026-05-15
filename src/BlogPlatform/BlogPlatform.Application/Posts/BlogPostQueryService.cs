using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Posts;

internal sealed class BlogPostQueryService : IBlogPostQueryService
{
    private readonly IBlogPostRepository _posts;

    public BlogPostQueryService(IBlogPostRepository posts)
    {
        _posts = posts;
    }

    public async Task<IReadOnlyCollection<PostListItem>> GetPublishedPostsAsync(
        string? categorySlug,
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPublishedPostsAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            posts = posts
                .Where(post => string.Equals(
                    post.CategorySlug,
                    categorySlug,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return posts
            .OrderByDescending(post => post.PublishedDate)
            .Select(ToListItem)
            .ToList();
    }

    public async Task<PostDetails?> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPublishedPostsAsync(cancellationToken);

        var post = posts.FirstOrDefault(item =>
            string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase));

        return post is null
            ? null
            : ToDetails(post);
    }

    public async Task<IReadOnlyCollection<CategorySummary>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPublishedPostsAsync(cancellationToken);

        return posts
            .GroupBy(post => new { post.CategorySlug, post.Category })
            .Where(group => !string.IsNullOrWhiteSpace(group.Key.CategorySlug))
            .Select(group => new CategorySummary(
                group.Key.CategorySlug,
                group.Key.Category,
                group.Count()))
            .OrderBy(category => category.Name)
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

    private static PostDetails ToDetails(Post post)
    {
        return new PostDetails(
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
            post.PublishedDate,
            post.BodyHtml);
    }
}
