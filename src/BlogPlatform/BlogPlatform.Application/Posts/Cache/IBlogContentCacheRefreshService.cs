namespace BlogPlatform.Application.Posts.Cache;

public interface IBlogContentCacheRefreshService
{
    Task<BlogContentCacheRefreshResult> RefreshAsync(
        CancellationToken cancellationToken);
}
