using System.Net.Http.Json;
using BlogPlatform.App.Models;

namespace BlogPlatform.App.Services;

public sealed class BlogApiClient : IBlogApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BlogApiClient> _logger;

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
        var url = string.IsNullOrWhiteSpace(categorySlug)
            ? "api/posts"
            : $"api/posts?category={Uri.EscapeDataString(categorySlug)}";

        _logger.LogInformation("APP calling API endpoint: {Url}", url);

        try
        {
            var posts = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<PostListItem>>(
                url,
                cancellationToken) ?? [];

            _logger.LogInformation("APP received posts from API. Count: {Count}", posts.Count);

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
        var url = $"api/posts/{Uri.EscapeDataString(slug)}";

        _logger.LogInformation("APP calling API endpoint: {Url}", url);

        try
        {
            var post = await _httpClient.GetFromJsonAsync<PostDetails>(
                url,
                cancellationToken);

            _logger.LogInformation(
                "APP received post details from API. Slug: {Slug}. Found: {Found}",
                slug,
                post is not null);

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
        const string url = "api/posts/categories";

        _logger.LogInformation("APP calling API endpoint: {Url}", url);

        try
        {
            var categories = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CategoryItem>>(
                url,
                cancellationToken) ?? [];

            _logger.LogInformation("APP received categories from API. Count: {Count}", categories.Count);

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "APP failed to get categories from API. Url: {Url}", url);
            throw;
        }
    }
}
