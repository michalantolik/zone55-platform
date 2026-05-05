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
        if (_cache.TryGetValue(PostsCacheKey, out List<PostDetailsDto>? cachedPosts) &&
            cachedPosts is not null)
        {
            _logger.LogDebug("API CMS posts cache hit. Count: {Count}", cachedPosts.Count);

            return cachedPosts;
        }

        await PostsLoadLock.WaitAsync(cancellationToken);

        try
        {
            if (_cache.TryGetValue(PostsCacheKey, out cachedPosts) &&
                cachedPosts is not null)
            {
                _logger.LogDebug(
                    "API CMS posts cache hit after wait. Count: {Count}",
                    cachedPosts.Count);

                return cachedPosts;
            }

            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "API CMS posts cache miss. Calling CMS. Base address: {BaseAddress}. Endpoint: {Endpoint}",
                _httpClient.BaseAddress,
                _options.PostsEndpoint);

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

            var posts = await response.Content.ReadFromJsonAsync<List<PostDetailsDto>>(
                cancellationToken: cancellationToken) ?? [];

            stopwatch.Stop();

            _cache.Set(
                PostsCacheKey,
                posts,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(3),
                    Priority = CacheItemPriority.High
                });

            _logger.LogInformation(
                "API received CMS posts. Count: {Count}. Duration: {ElapsedMs} ms",
                posts.Count,
                stopwatch.ElapsedMilliseconds);

            return posts;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "API failed while calling CMS content endpoint. Base address: {BaseAddress}. Endpoint: {Endpoint}",
                _httpClient.BaseAddress,
                _options.PostsEndpoint);

            throw;
        }
        finally
        {
            PostsLoadLock.Release();
        }
    }
}
