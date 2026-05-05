using BlogPlatform.Cms.Seeding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace BlogPlatform.Cms.Controllers;

[ApiController]
[Route("api/blog-content")]
public sealed class BlogContentController : ControllerBase
{
    private const string ArticlesCacheKey = "cms-blog-articles";

    private static readonly SemaphoreSlim ArticlesLoadLock = new(1, 1);

    private readonly IContentService _contentService;
    private readonly ILogger<BlogContentController> _logger;
    private readonly IMemoryCache _cache;

    public BlogContentController(
        IContentService contentService,
        ILogger<BlogContentController> logger,
        IMemoryCache cache)
    {
        _contentService = contentService;
        _logger = logger;
        _cache = cache;
    }

    [HttpGet("articles")]
    public async Task<ActionResult<IReadOnlyCollection<CmsPostDetailsDto>>> GetArticles(
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(ArticlesCacheKey, out IReadOnlyCollection<CmsPostDetailsDto>? cachedArticles) &&
            cachedArticles is not null)
        {
            _logger.LogDebug("CMS blog articles cache hit. Count: {Count}", cachedArticles.Count);

            return Ok(cachedArticles);
        }

        await ArticlesLoadLock.WaitAsync(cancellationToken);

        try
        {
            if (_cache.TryGetValue(ArticlesCacheKey, out cachedArticles) &&
                cachedArticles is not null)
            {
                _logger.LogDebug(
                    "CMS blog articles cache hit after wait. Count: {Count}",
                    cachedArticles.Count);

                return Ok(cachedArticles);
            }

            _logger.LogInformation("CMS blog articles cache miss. Loading blog articles.");

            var rootContent = _contentService.GetRootContent().ToList();

            var categories = rootContent
                .Where(content => content.ContentType.Alias == BlogContentAliases.BlogCategory)
                .ToDictionary(
                    content => content.Key,
                    content => new CmsCategoryDto(
                        GetString(content, BlogContentAliases.Slug) ?? CreateSlug(content.Name ?? "uncategorized"),
                        GetString(content, BlogContentAliases.Title) ?? content.Name ?? "Uncategorized"));

            var posts = rootContent
                .Where(content => content.ContentType.Alias == BlogContentAliases.BlogArticle)
                .Select(content => MapPost(content, categories))
                .OrderByDescending(post => post.PublishedDate)
                .ToList();

            _cache.Set(
                ArticlesCacheKey,
                posts,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(3),
                    Priority = CacheItemPriority.High
                });

            _logger.LogInformation("CMS loaded blog articles. Count: {Count}", posts.Count);

            return Ok(posts);
        }
        finally
        {
            ArticlesLoadLock.Release();
        }
    }

    private static CmsPostDetailsDto MapPost(
        IContent content,
        IReadOnlyDictionary<Guid, CmsCategoryDto> categories)
    {
        var category = ResolveCategory(content, categories);

        var tags = GetString(content, BlogContentAliases.Tags)?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToArray() ?? [];

        var title = GetString(content, BlogContentAliases.Title)
            ?? content.Name
            ?? "Untitled";

        return new CmsPostDetailsDto(
            GetString(content, BlogContentAliases.Slug) ?? CreateSlug(title),
            title,
            GetString(content, BlogContentAliases.Summary) ?? string.Empty,
            category.Name,
            category.Slug,
            GetString(content, BlogContentAliases.Level) ?? "Intermediate",
            GetString(content, BlogContentAliases.Focus) ?? "Practical",
            tags,
            GetDateTimeOffset(content, BlogContentAliases.PublishedDate),
            string.Empty);
    }

    private static CmsCategoryDto ResolveCategory(
        IContent content,
        IReadOnlyDictionary<Guid, CmsCategoryDto> categories)
    {
        var rawCategory = GetString(content, BlogContentAliases.Category);

        if (!string.IsNullOrWhiteSpace(rawCategory))
        {
            var guidText = rawCategory
                .Replace("umb://document/", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);

            if (Guid.TryParse(guidText, out var categoryKey) &&
                categories.TryGetValue(categoryKey, out var category))
            {
                return category;
            }
        }

        return new CmsCategoryDto("uncategorized", "Uncategorized");
    }

    private static string? GetString(IContent content, string alias)
    {
        return content.GetValue<string>(alias);
    }

    private static DateTimeOffset? GetDateTimeOffset(IContent content, string alias)
    {
        var value = content.GetValue<DateTime?>(alias);

        return value.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
            : null;
    }

    private static string CreateSlug(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-");
    }

    private sealed record CmsCategoryDto(
        string Slug,
        string Name);

    public sealed record CmsPostDetailsDto(
        string Slug,
        string Title,
        string Summary,
        string Category,
        string CategorySlug,
        string Level,
        string Focus,
        IReadOnlyCollection<string> Tags,
        DateTimeOffset? PublishedDate,
        string BodyHtml);
}
