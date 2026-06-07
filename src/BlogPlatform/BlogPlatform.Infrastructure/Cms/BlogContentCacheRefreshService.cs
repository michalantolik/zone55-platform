using BlogPlatform.Application.Posts;
using BlogPlatform.Application.Posts.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Cms;

public sealed class BlogContentCacheRefreshService : IBlogContentCacheRefreshService
{
    private readonly IMemoryCache _cache;
    private readonly IBlogPostRepository _posts;
    private readonly ILogger<BlogContentCacheRefreshService> _logger;

    public BlogContentCacheRefreshService(
        IMemoryCache cache,
        IBlogPostRepository posts,
        ILogger<BlogContentCacheRefreshService> logger)
    {
        _cache = cache;
        _posts = posts;
        _logger = logger;
    }

    public async Task<BlogContentCacheRefreshResult> RefreshAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("API blog content cache refresh requested.");

        _cache.Remove(BlogContentCacheKeys.Posts);

        var posts = await _posts.GetPostsAsync(cancellationToken);

        return new BlogContentCacheRefreshResult(
            true,
            "Blog content cache refreshed successfully.",
            posts.Count,
            DateTimeOffset.UtcNow);
    }
}
