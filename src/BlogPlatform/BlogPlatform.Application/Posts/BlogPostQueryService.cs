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
        GetPublishedPostsQuery query,
        CancellationToken cancellationToken)
    {
        var posts = await GetPublishedDomainPostsAsync(cancellationToken);

        return posts
            .FilterByCategorySlug(query.NormalizedCategorySlug)
            .OrderByDescending(post => post.PublishedDate)
            .Select(BlogPostApplicationMapper.ToListItem)
            .ToList();
    }

    public async Task<PostDetails?> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var posts = await GetPublishedDomainPostsAsync(cancellationToken);

        var post = posts.FirstOrDefault(item =>
            string.Equals(item.Slug, slug.Trim(), StringComparison.OrdinalIgnoreCase));

        return post is null
            ? null
            : BlogPostApplicationMapper.ToDetails(post);
    }

    public async Task<IReadOnlyCollection<CategorySummary>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var posts = await GetPublishedDomainPostsAsync(cancellationToken);

        return posts.ToCategorySummaries();
    }

    private async Task<IReadOnlyCollection<Post>> GetPublishedDomainPostsAsync(
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPostsAsync(cancellationToken);

        return posts
            .Where(post => post.IsPublished)
            .ToList();
    }
}

internal static class BlogPostQueryExtensions
{
    public static IEnumerable<Post> FilterByCategorySlug(
        this IEnumerable<Post> posts,
        string? categorySlug)
    {
        if (string.IsNullOrWhiteSpace(categorySlug))
        {
            return posts;
        }

        return posts.Where(post => string.Equals(
            post.CategorySlug,
            categorySlug,
            StringComparison.OrdinalIgnoreCase));
    }

    public static IReadOnlyCollection<CategorySummary> ToCategorySummaries(
        this IEnumerable<Post> posts)
    {
        return posts
            .GroupBy(post => new { post.CategorySlug, post.Category })
            .Where(group => !string.IsNullOrWhiteSpace(group.Key.CategorySlug))
            .Select(group => BlogPostApplicationMapper.ToCategorySummary(
                group.Key.CategorySlug,
                group.Key.Category,
                group.Count()))
            .OrderBy(category => category.Name)
            .ToList();
    }
}
