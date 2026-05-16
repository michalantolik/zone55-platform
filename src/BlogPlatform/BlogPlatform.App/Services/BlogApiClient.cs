using BlogPlatform.App.Models;
using BlogPlatform.Contracts.DotnetRoadmap;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;

namespace BlogPlatform.App.Services;

public sealed class BlogApiClient : IBlogApiClient
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private readonly HttpClient _httpClient;
    private readonly ILogger<BlogApiClient> _logger;

    private readonly ConcurrentDictionary<string, HomeCacheEntry> _homeCache = [];
    private readonly ConcurrentDictionary<string, Lazy<Task<BlogHomeContent>>> _homeRequests = [];
    private readonly ConcurrentDictionary<string, PostDetailsCacheEntry> _postDetailsCache = [];

    public BlogApiClient(
        HttpClient httpClient,
        ILogger<BlogApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<RoadmapZoneDto>> GetDotnetRoadmapAsync(
    CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyCollection<RoadmapZoneDto>>(
            "api/roadmap/dotnet",
            cancellationToken) ?? [];
    }

    public async Task<BlogHomeContent> GetHomeContentAsync(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = "__home__";

        if (_homeCache.TryGetValue(cacheKey, out var cachedHome) &&
            cachedHome.ExpiresAt > DateTimeOffset.UtcNow)
        {
            return cachedHome.Content;
        }

        var request = _homeRequests.GetOrAdd(
            cacheKey,
            _ => new Lazy<Task<BlogHomeContent>>(
                () => LoadHomeContentAsync(cacheKey, cancellationToken)));

        try
        {
            return await request.Value;
        }
        finally
        {
            _homeRequests.TryRemove(cacheKey, out _);
        }
    }

    public async Task<IReadOnlyCollection<PostListItem>> GetPostsAsync(
        CancellationToken cancellationToken = default)
    {
        var homeContent = await GetHomeContentAsync(cancellationToken);

        return homeContent.Posts;
    }

    public async Task<PostDetails?> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = slug.Trim().ToLowerInvariant();

        if (_postDetailsCache.TryGetValue(cacheKey, out var cachedPost) &&
            cachedPost.ExpiresAt > DateTimeOffset.UtcNow)
        {
            return cachedPost.Post;
        }

        var url = $"api/posts/{Uri.EscapeDataString(slug)}";

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _postDetailsCache[cacheKey] = new PostDetailsCacheEntry(
                    null,
                    DateTimeOffset.UtcNow.Add(CacheDuration));

                return null;
            }

            response.EnsureSuccessStatusCode();

            var post = await response.Content.ReadFromJsonAsync<PostDetails>(
                cancellationToken: cancellationToken);

            _postDetailsCache[cacheKey] = new PostDetailsCacheEntry(
                post,
                DateTimeOffset.UtcNow.Add(CacheDuration));

            return post;
        }
        catch (Exception ex) when (_postDetailsCache.TryGetValue(cacheKey, out var stalePost))
        {
            _logger.LogWarning(
                ex,
                "APP failed to refresh post details from API. Returning cached post. Url: {Url}",
                url);

            return stalePost.Post;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "APP failed to get post details from API. Url: {Url}", url);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<PostListItem>> GetPostsByStepAsync(
        string zone,
        string step,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"api/posts/by-step?zone={Uri.EscapeDataString(zone)}&step={Uri.EscapeDataString(step)}";

        return await _httpClient.GetFromJsonAsync<IReadOnlyCollection<PostListItem>>(
            url,
            cancellationToken) ?? [];
    }
    private async Task<BlogHomeContent> LoadHomeContentAsync(
        string cacheKey,
        CancellationToken cancellationToken)
    {
        const string url = "api/posts/home";

        try
        {
            var content = await _httpClient.GetFromJsonAsync<BlogHomeContent>(
                url,
                cancellationToken) ?? new BlogHomeContent([]);

            _homeCache[cacheKey] = new HomeCacheEntry(
                content,
                DateTimeOffset.UtcNow.Add(CacheDuration));

            return content;
        }
        catch (Exception ex) when (_homeCache.TryGetValue(cacheKey, out var staleHome))
        {
            _logger.LogWarning(
                ex,
                "APP failed to refresh home content from API. Returning cached content. Url: {Url}",
                url);

            return staleHome.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "APP failed to get home content from API. Url: {Url}", url);
            throw;
        }
    }

    private sealed record HomeCacheEntry(
        BlogHomeContent Content,
        DateTimeOffset ExpiresAt);

    private sealed record PostDetailsCacheEntry(
        PostDetails? Post,
        DateTimeOffset ExpiresAt);
}
