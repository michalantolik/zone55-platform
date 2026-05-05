using System.Net.Http.Json;
using BlogPlatform.App.Models;

namespace BlogPlatform.App.Services;

public sealed class BlogApiClient : IBlogApiClient
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private readonly HttpClient _httpClient;
    private readonly ILogger<BlogApiClient> _logger;

    private IReadOnlyCollection<CategoryItem>? _categoriesCache;
    private DateTimeOffset _categoriesCacheExpiresAt;

    private readonly Dictionary<string, PostsCacheEntry> _postsCache = [];
    private readonly Dictionary<string, PostDetailsCacheEntry> _postDetailsCache = [];

    public BlogApiClient(
        HttpClient httpClient,
        ILogger<BlogApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<PostListItem>> GetPostsAsync(
        string? categorySlug,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = string.IsNullOrWhiteSpace(categorySlug)
            ? "__all__"
            : categorySlug.Trim().ToLowerInvariant();

        if (_postsCache.TryGetValue(cacheKey, out var cachedPosts) &&
            cachedPosts.ExpiresAt > DateTimeOffset.UtcNow)
        {
            return cachedPosts.Posts;
        }

        var url = string.IsNullOrWhiteSpace(categorySlug)
            ? "api/posts"
            : $"api/posts?category={Uri.EscapeDataString(categorySlug)}";

        try
        {
            var posts = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<PostListItem>>(
                url,
                cancellationToken) ?? [];

            _postsCache[cacheKey] = new PostsCacheEntry(
                posts,
                DateTimeOffset.UtcNow.Add(CacheDuration));

            return posts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "APP failed to get posts from API. Url: {Url}", url);
            throw;
        }
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
            var post = await _httpClient.GetFromJsonAsync<PostDetails>(
                url,
                cancellationToken);

            _postDetailsCache[cacheKey] = new PostDetailsCacheEntry(
                post,
                DateTimeOffset.UtcNow.Add(CacheDuration));

            return post;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "APP failed to get post details from API. Url: {Url}", url);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<CategoryItem>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_categoriesCache is not null &&
            _categoriesCacheExpiresAt > DateTimeOffset.UtcNow)
        {
            return _categoriesCache;
        }

        const string url = "api/posts/categories";

        try
        {
            var categories = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CategoryItem>>(
                url,
                cancellationToken) ?? [];

            _categoriesCache = categories;
            _categoriesCacheExpiresAt = DateTimeOffset.UtcNow.Add(CacheDuration);

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "APP failed to get categories from API. Url: {Url}", url);
            throw;
        }
    }

    private sealed record PostsCacheEntry(
        IReadOnlyCollection<PostListItem> Posts,
        DateTimeOffset ExpiresAt);

    private sealed record PostDetailsCacheEntry(
        PostDetails? Post,
        DateTimeOffset ExpiresAt);
}
