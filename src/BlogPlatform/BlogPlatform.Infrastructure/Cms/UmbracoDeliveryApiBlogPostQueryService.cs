using System.Diagnostics;
using System.Net.Http.Json;
using BlogPlatform.Application.Posts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlogPlatform.Infrastructure.Cms;

internal sealed class UmbracoDeliveryApiBlogPostQueryService : IBlogPostQueryService
{
    private const string PostsCacheKey = "cms-post-details";

    private static readonly SemaphoreSlim PostsLoadLock = new(1, 1);

    private readonly HttpClient _httpClient;
    private readonly UmbracoDeliveryApiOptions _options;
    private readonly ILogger<UmbracoDeliveryApiBlogPostQueryService> _logger;
    private readonly IMemoryCache _cache;

    public UmbracoDeliveryApiBlogPostQueryService(
        HttpClient httpClient,
        IOptions<UmbracoDeliveryApiOptions> options,
        ILogger<UmbracoDeliveryApiBlogPostQueryService> logger,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<PostListItemDto>> GetPublishedPostsAsync(
        string? categorySlug,
        CancellationToken cancellationToken)
    {
        var posts = await LoadPostDetailsAsync(cancellationToken);

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
            .Select(post => new PostListItemDto(
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
                post.PublishedDate))
            .ToList();
    }

    public async Task<PostDetailsDto?> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        var posts = await LoadPostDetailsAsync(cancellationToken);

        return posts.FirstOrDefault(post =>
            string.Equals(post.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var posts = await LoadPostDetailsAsync(cancellationToken);

        return posts
            .GroupBy(post => new { post.CategorySlug, post.Category })
            .Where(group => !string.IsNullOrWhiteSpace(group.Key.CategorySlug))
            .Select(group => new CategoryDto(
                group.Key.CategorySlug,
                group.Key.Category,
                group.Count()))
            .OrderBy(category => category.Name)
            .ToList();
    }

    private async Task<List<PostDetailsDto>> LoadPostDetailsAsync(
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        if (_cache.TryGetValue(PostsCacheKey, out PostsCacheEntry? cachedEntry) &&
            cachedEntry is not null &&
            cachedEntry.FreshUntil > now)
        {
            _logger.LogDebug("API CMS posts fresh cache hit. Count: {Count}", cachedEntry.Posts.Count);

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

    private async Task<List<PostDetailsDto>> FetchPostsFromCmsAsync(
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

        return await response.Content.ReadFromJsonAsync<List<PostDetailsDto>>(
            cancellationToken: cancellationToken) ?? [];
    }

    private sealed record PostsCacheEntry(
        List<PostDetailsDto> Posts,
        DateTimeOffset FreshUntil);
}
