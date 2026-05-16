using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Posts;

public sealed class BlogPostQueryService : IBlogPostQueryService
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

    public async Task<IReadOnlyCollection<PostListItem>> GetPublishedPostsForStepAsync(
        GetPublishedPostsForStepQuery query,
        CancellationToken cancellationToken)
    {
        var posts = await GetPublishedDomainPostsAsync(cancellationToken);

        return posts
            .Where(post =>
                string.Equals(post.DotnetZone, query.NormalizedDotnetZone, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(post.DotnetZoneStep, query.NormalizedDotnetZoneStep, StringComparison.OrdinalIgnoreCase))
            .OrderBy(post => GetLevelOrder(post.Level))
            .ThenByDescending(post => post.PublishedDate)
            .ThenBy(post => post.Title)
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

    private static int GetLevelOrder(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            return 50;
        }

        if (level.Contains("beginner", StringComparison.OrdinalIgnoreCase) ||
            level.Contains("basic", StringComparison.OrdinalIgnoreCase) ||
            level.Contains("fundamental", StringComparison.OrdinalIgnoreCase))
        {
            return 10;
        }

        if (level.Contains("intermediate", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (level.Contains("advanced", StringComparison.OrdinalIgnoreCase))
        {
            return 30;
        }

        return 50;
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
