using System.Net.Http.Json;
using BlogPlatform.App.Models;

namespace BlogPlatform.App.Services;

public sealed class BlogApiClient : IBlogApiClient
{
    private readonly HttpClient _httpClient;

    public BlogApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<PostListItem>> GetPostsAsync(
        string? categorySlug,
        CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(categorySlug)
            ? "api/posts"
            : $"api/posts?category={Uri.EscapeDataString(categorySlug)}";

        return await _httpClient.GetFromJsonAsync<IReadOnlyCollection<PostListItem>>(
            url,
            cancellationToken) ?? [];
    }

    public async Task<PostDetails?> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<PostDetails>(
            $"api/posts/{Uri.EscapeDataString(slug)}",
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<CategoryItem>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CategoryItem>>(
            "api/posts/categories",
            cancellationToken) ?? [];
    }
}
