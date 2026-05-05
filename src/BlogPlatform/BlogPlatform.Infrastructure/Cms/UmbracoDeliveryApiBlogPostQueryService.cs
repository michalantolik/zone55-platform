using System.Net.Http.Json;
using System.Text.Json;
using BlogPlatform.Application.Posts;
using Microsoft.Extensions.Options;

namespace BlogPlatform.Infrastructure.Cms;

internal sealed class UmbracoDeliveryApiBlogPostQueryService : IBlogPostQueryService
{
    private readonly HttpClient _httpClient;
    private readonly UmbracoDeliveryApiOptions _options;

    public UmbracoDeliveryApiBlogPostQueryService(
        HttpClient httpClient,
        IOptions<UmbracoDeliveryApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyCollection<PostListItemDto>> GetPublishedPostsAsync(
        string? categorySlug,
        CancellationToken cancellationToken)
    {
        var posts = await LoadPostsAsync(cancellationToken);

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
        var posts = await LoadPostsAsync(cancellationToken);

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

    private async Task<List<PostListItemDto>> LoadPostsAsync(
        CancellationToken cancellationToken)
    {
        var details = await LoadPostDetailsAsync(cancellationToken);

        return details
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

    private async Task<List<PostDetailsDto>> LoadPostDetailsAsync(
        CancellationToken cancellationToken)
    {
        using var document = await _httpClient.GetFromJsonAsync<JsonDocument>(
            _options.PostsEndpoint,
            cancellationToken);

        if (document is null)
        {
            return [];
        }

        var root = document.RootElement;

        if (!root.TryGetProperty("items", out var items) ||
            items.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var posts = new List<PostDetailsDto>();

        foreach (var item in items.EnumerateArray())
        {
            posts.Add(MapPost(item));
        }

        return posts;
    }

    private static PostDetailsDto MapPost(JsonElement item)
    {
        var properties = item.TryGetProperty("properties", out var props)
            ? props
            : default;

        var title = GetString(properties, "title")
            ?? GetString(item, "name")
            ?? "Untitled";

        var slug = GetString(properties, "slug")
            ?? CreateSlug(title);

        var categoryName = "Uncategorized";
        var categorySlug = "uncategorized";

        if (properties.ValueKind != JsonValueKind.Undefined &&
            properties.TryGetProperty("category", out var category))
        {
            categoryName =
                GetString(category, "name")
                ?? GetString(category, "title")
                ?? GetNestedString(category, "properties", "title")
                ?? categoryName;

            categorySlug =
                GetString(category, "slug")
                ?? GetNestedString(category, "properties", "slug")
                ?? CreateSlug(categoryName);
        }

        var tags = GetStringArray(properties, "tags");

        return new PostDetailsDto(
            slug,
            title,
            GetString(properties, "summary") ?? string.Empty,
            categoryName,
            categorySlug,
            GetString(properties, "level") ?? "Intermediate",
            GetString(properties, "focus") ?? "Practical",
            tags,
            GetDateTimeOffset(properties, "publishedDate"),
            string.Empty);
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Undefined ||
            !element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static string? GetNestedString(
        JsonElement element,
        string objectProperty,
        string valueProperty)
    {
        if (element.ValueKind == JsonValueKind.Undefined ||
            !element.TryGetProperty(objectProperty, out var nested))
        {
            return null;
        }

        return GetString(nested, valueProperty);
    }

    private static IReadOnlyCollection<string> GetStringArray(
        JsonElement element,
        string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Undefined ||
            !element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return property
            .EnumerateArray()
            .Where(value => value.ValueKind == JsonValueKind.String)
            .Select(value => value.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();
    }

    private static DateTimeOffset? GetDateTimeOffset(
        JsonElement element,
        string propertyName)
    {
        var value = GetString(element, propertyName);

        return DateTimeOffset.TryParse(value, out var result)
            ? result
            : null;
    }

    private static string CreateSlug(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-");
    }
}
