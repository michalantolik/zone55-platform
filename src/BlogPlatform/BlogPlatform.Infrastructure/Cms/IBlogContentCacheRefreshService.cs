namespace BlogPlatform.Infrastructure.Cms;

public interface IBlogContentCacheRefreshService
{
    Task<BlogContentCacheRefreshResult> RefreshAsync(
        CancellationToken cancellationToken);
}
