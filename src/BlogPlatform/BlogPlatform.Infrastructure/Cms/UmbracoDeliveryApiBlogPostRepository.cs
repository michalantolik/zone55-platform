using BlogPlatform.Application.Posts;
using BlogPlatform.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Json;

namespace BlogPlatform.Infrastructure.Cms;

public sealed class UmbracoDeliveryApiBlogPostRepository : IBlogPostRepository
{
    private const string PostsCacheKey = "cms-post-details";

    private static readonly SemaphoreSlim PostsLoadLock = new(1, 1);

    private readonly HttpClient _httpClient;
    private readonly UmbracoDeliveryApiOptions _options;
    private readonly ILogger<UmbracoDeliveryApiBlogPostRepository> _logger;
    private readonly IMemoryCache _cache;

    public UmbracoDeliveryApiBlogPostRepository(
        HttpClient httpClient,
        IOptions<UmbracoDeliveryApiOptions> options,
        ILogger<UmbracoDeliveryApiBlogPostRepository> logger,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<Post>> GetPostsAsync(
        CancellationToken cancellationToken)
    {
        return await LoadPostsAsync(cancellationToken);
    }

    private async Task<List<Post>> LoadPostsAsync(
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        if (_cache.TryGetValue(PostsCacheKey, out PostsCacheEntry? cachedEntry) &&
            cachedEntry is not null &&
            cachedEntry.FreshUntil > now)
        {
            _logger.LogDebug(
                "API CMS posts fresh cache hit. Count: {Count}",
                cachedEntry.Posts.Count);

            return cachedEntry.Posts;
        }

        await PostsLoadLock.WaitAsync(cancellationToken);

        try
        {
            now = DateTimeOffset.UtcNow;

            if (_cache.TryGetValue(PostsCacheKey, out cachedEntry) &&
                cachedEntry is not null &&
                cachedEntry.FreshUntil > now)
            {
                _logger.LogDebug(
                    "API CMS posts fresh cache hit after wait. Count: {Count}",
                    cachedEntry.Posts.Count);

                return cachedEntry.Posts;
            }

            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "API CMS posts cache miss or stale cache. Calling CMS. Base address: {BaseAddress}. Endpoint: {Endpoint}",
                _httpClient.BaseAddress,
                _options.PostsEndpoint);

            try
            {
                var posts = await FetchPostsFromCmsAsync(stopwatch, cancellationToken);

                var freshCacheDuration = TimeSpan.FromSeconds(_options.FreshCacheSeconds);
                var staleCacheDuration = TimeSpan.FromSeconds(_options.StaleCacheSeconds);

                _cache.Set(
                    PostsCacheKey,
                    new PostsCacheEntry(posts, DateTimeOffset.UtcNow.Add(freshCacheDuration)),
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = staleCacheDuration,
                        Priority = CacheItemPriority.High
                    });

                _logger.LogInformation(
                    "API received CMS posts. Count: {Count}. Duration: {ElapsedMs} ms",
                    posts.Count,
                    stopwatch.ElapsedMilliseconds);

                return posts;
            }
            catch (Exception ex) when (cachedEntry is not null)
            {
                _logger.LogWarning(
                    ex,
                    "API failed to refresh CMS posts. Returning stale cache. Count: {Count}",
                    cachedEntry.Posts.Count);

                return cachedEntry.Posts;
            }
        }
        finally
        {
            PostsLoadLock.Release();
        }
    }

    private async Task<List<Post>> FetchPostsFromCmsAsync(
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            _options.PostsEndpoint,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogError(
                "CMS content endpoint failed. Status: {StatusCode}. Duration: {ElapsedMs} ms. Body: {ResponseBody}",
                response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                responseBody);

            response.EnsureSuccessStatusCode();
        }

        var cmsPosts = await response.Content.ReadFromJsonAsync<List<CmsPostDto>>(
            cancellationToken: cancellationToken);

        var posts = cmsPosts?
            .Select(ToDomainPost)
            .ToList() ?? [];

        _logger.LogInformation(
            "API mapped CMS article DTOs to domain posts. Count: {Count}",
            posts.Count);

        return posts;
    }

    private static Post ToDomainPost(CmsPostDto post)
    {
        return Post.CreatePublished(
            post.Slug,
            post.Title,
            post.Summary,
            post.Level,
            post.Focus,
            post.DotnetZone,
            post.DotnetZoneStep,
            post.Tags,
            post.PublishedDate,
            post.BodyHtml);
    }

    private sealed record PostsCacheEntry(
        List<Post> Posts,
        DateTimeOffset FreshUntil);

    private sealed record CmsPostDto(
        Guid Key,
        string? Slug,
        string? Title,
        string? Summary,
        string? Level,
        string? Focus,
        string? DotnetZone,
        string? DotnetZoneStep,
        IReadOnlyCollection<string>? Tags,
        DateTimeOffset? PublishedDate,
        string? BodyHtml,
        DateTime UpdatedUtc);
}
