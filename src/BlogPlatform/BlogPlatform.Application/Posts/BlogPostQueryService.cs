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
            .Where(post => post.MatchesDotnetStep(
                query.NormalizedDotnetZone,
                query.NormalizedDotnetZoneStep))
            .OrderBy(post => PostLevelSortOrder.FromLevel(post.Level))
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

    private async Task<IReadOnlyCollection<Post>> GetPublishedDomainPostsAsync(
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPostsAsync(cancellationToken);

        return posts
            .Where(post => post.IsPublished)
            .ToList();
    }
}
