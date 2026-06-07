using BlogPlatform.Application.Posts.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/blog-content-cache")]
public sealed class BlogContentCacheController : ControllerBase
{
    private const string HeaderName = "X-BlogPlatform-Seed-Key";

    private readonly IBlogContentCacheRefreshService _cacheRefresh;
    private readonly BlogContentCacheOperationsOptions _options;

    public BlogContentCacheController(
        IBlogContentCacheRefreshService cacheRefresh,
        IOptions<BlogContentCacheOperationsOptions> options)
    {
        _cacheRefresh = cacheRefresh;
        _options = options.Value;
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<BlogContentCacheRefreshResponse>> Refresh(
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                BlogContentCacheRefreshResponse.Failed(
                    "Cache refresh API key is not configured."));
        }

        if (!Request.Headers.TryGetValue(HeaderName, out var providedKey) ||
            providedKey != _options.ApiKey)
        {
            return Unauthorized(BlogContentCacheRefreshResponse.Failed(
                "Invalid cache refresh API key."));
        }

        var result = await _cacheRefresh.RefreshAsync(cancellationToken);

        return Ok(new BlogContentCacheRefreshResponse(
            result.Success,
            result.Message,
            result.Posts,
            result.CompletedAtUtc));
    }
}

public sealed class BlogContentCacheOperationsOptions
{
    public string ApiKey { get; set; } = string.Empty;
}

public sealed record BlogContentCacheRefreshResponse(
    bool Success,
    string Message,
    int Posts,
    DateTimeOffset CompletedAtUtc)
{
    public static BlogContentCacheRefreshResponse Failed(string message)
    {
        return new BlogContentCacheRefreshResponse(
            false,
            message,
            0,
            DateTimeOffset.UtcNow);
    }
}
