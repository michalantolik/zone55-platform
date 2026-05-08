using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
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
    private const string CategoriesCacheKey = "cms-blog-categories";

    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<BlogContentController> _logger;
    private readonly IMemoryCache _cache;

    public BlogContentController(
        IContentService contentService,
        IContentTypeService contentTypeService,
        ILogger<BlogContentController> logger,
        IMemoryCache cache)
    {
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _logger = logger;
        _cache = cache;
    }

    [HttpGet("categories")]
    public ActionResult<IReadOnlyCollection<CmsCategoryListItemDto>> GetCategories()
    {
        var categories = GetRootCategories()
            .Select(content => new CmsCategoryListItemDto(
                content.Key,
                GetString(content, BlogContentAliases.Title) ?? content.Name ?? "Untitled category",
                GetString(content, BlogContentAliases.Slug) ?? CreateSlug(content.Name ?? "category")))
            .OrderBy(category => category.Name)
            .ToList();

        return Ok(categories);
    }

    [HttpGet("document-types")]
    public ActionResult<IReadOnlyCollection<CmsDocumentTypeListItemDto>> GetDocumentTypes()
    {
        var documentTypes = _contentTypeService
            .GetAll()
            .Select(contentType => new CmsDocumentTypeListItemDto(
                contentType.Key,
                contentType.Name ?? contentType.Alias,
                contentType.Alias,
                contentType.Icon ?? "icon-document",
                contentType.IsElement,
                GetContentCount(contentType.Alias)))
            .OrderBy(documentType => documentType.IsElement)
            .ThenBy(documentType => documentType.Name)
            .ToList();

        return Ok(documentTypes);
    }

    [HttpGet("articles")]
    public ActionResult<IReadOnlyCollection<CmsArticleListItemDto>> GetArticles(
        [FromQuery] string? categorySlug = null)
    {
        var categories = GetCategoryDictionary();

        var posts = GetRootArticles()
            .Select(content => MapArticleListItem(content, categories))
            .Where(article =>
                string.IsNullOrWhiteSpace(categorySlug) ||
                string.Equals(article.CategorySlug, categorySlug, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(article => article.PublishedDate)
            .ThenBy(article => article.Title)
            .ToList();

        return Ok(posts);
    }

    [HttpGet("articles/{key:guid}")]
    public ActionResult<CmsArticleEditorDto> GetArticle(Guid key)
    {
        var article = _contentService.GetById(key);

        if (article is null ||
            article.ContentType.Alias != BlogContentAliases.BlogArticle)
        {
            return NotFound();
        }

        var categories = GetCategoryDictionary();
        var category = ResolveCategory(article, categories);

        return Ok(new CmsArticleEditorDto(
            article.Key,
            GetString(article, BlogContentAliases.Title) ?? article.Name ?? "Untitled",
            GetString(article, BlogContentAliases.Slug) ?? CreateSlug(article.Name ?? "article"),
            GetString(article, BlogContentAliases.Summary) ?? string.Empty,
            category.Name,
            category.Slug,
            GetString(article, BlogContentAliases.Level) ?? "Draft",
            GetString(article, BlogContentAliases.Focus) ?? "Preview",
            GetString(article, BlogContentAliases.Tags) ?? string.Empty,
            GetString(article, BlogContentAliases.BodyBlocks) ?? string.Empty,
            true));
    }


    [HttpDelete("categories/{key:guid}")]
    public IActionResult DeleteCategory(Guid key)
    {
        var category = _contentService.GetById(key);

        if (category is null ||
            category.ContentType.Alias != BlogContentAliases.BlogCategory)
        {
            return NotFound(new CmsDeleteResponse(false, "Category not found."));
        }

        var articlesUsingCategory = GetRootArticles()
            .Count(article => IsArticleAssignedToCategory(article, key));

        if (articlesUsingCategory > 0)
        {
            return BadRequest(new CmsDeleteResponse(
                false,
                $"Cannot delete category because {articlesUsingCategory} article{(articlesUsingCategory == 1 ? string.Empty : "s")} still use it."));
        }

        try
        {
            _contentService.Delete(category);
            ClearCaches();

            return Ok(new CmsDeleteResponse(true, "Category deleted successfully."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to delete CMS category. Key={CategoryKey}", key);
            return StatusCode(500, new CmsDeleteResponse(false, "Unable to delete category."));
        }
    }

    [HttpDelete("articles/{key:guid}")]
    public IActionResult DeleteArticle(Guid key)
    {
        var article = _contentService.GetById(key);

        if (article is null ||
            article.ContentType.Alias != BlogContentAliases.BlogArticle)
        {
            return NotFound(new CmsDeleteResponse(false, "Article not found."));
        }

        try
        {
            _contentService.Delete(article);
            ClearCaches();

            return Ok(new CmsDeleteResponse(true, "Article deleted successfully."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to delete CMS article. Key={ArticleKey}", key);
            return StatusCode(500, new CmsDeleteResponse(false, "Unable to delete article."));
        }
    }

    [HttpDelete("document-types/{key:guid}")]
    public IActionResult DeleteDocumentType(Guid key)
    {
        var documentType = _contentTypeService.Get(key);

        if (documentType is null)
        {
            return NotFound(new CmsDeleteResponse(false, "Document type not found."));
        }

        var contentCount = GetContentCount(documentType.Alias);

        if (contentCount > 0)
        {
            return BadRequest(new CmsDeleteResponse(
                false,
                $"Cannot delete document type because {contentCount} content item{(contentCount == 1 ? string.Empty : "s")} still use it."));
        }

        try
        {
            _contentTypeService.Delete(documentType);
            ClearCaches();

            return Ok(new CmsDeleteResponse(true, "Document type deleted successfully."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to delete CMS document type. Key={DocumentTypeKey}", key);
            return StatusCode(500, new CmsDeleteResponse(false, "Unable to delete document type."));
        }
    }

    [HttpPost("articles")]
    public ActionResult<CmsSaveArticleResponse> CreateArticle(
        [FromBody] CmsSaveArticleRequest request)
    {
        var category = FindCategoryBySlug(request.CategorySlug);

        if (category is null)
        {
            return BadRequest(new CmsSaveArticleResponse(
                false,
                Guid.Empty,
                "Category not found."));
        }

        var article = _contentService.Create(
            request.Title,
            -1,
            BlogContentAliases.BlogArticle);

        ApplyArticleValues(article, request, category);

        _contentService.Save(article);
        var result = _contentService.Publish(article, new[] { "*" });

        ClearCaches();

        return Ok(new CmsSaveArticleResponse(
            result.Success,
            article.Key,
            result.Success
                ? "Article created successfully."
                : "Unable to create article."));
    }

    [HttpPut("articles/{key:guid}")]
    public ActionResult<CmsSaveArticleResponse> UpdateArticle(
        Guid key,
        [FromBody] CmsSaveArticleRequest request)
    {
        var article = _contentService.GetById(key);

        if (article is null ||
            article.ContentType.Alias != BlogContentAliases.BlogArticle)
        {
            return NotFound();
        }

        var category = FindCategoryBySlug(request.CategorySlug);

        if (category is null)
        {
            return BadRequest(new CmsSaveArticleResponse(
                false,
                key,
                "Category not found."));
        }

        ApplyArticleValues(article, request, category);

        _contentService.Save(article);
        var result = _contentService.Publish(article, new[] { "*" });

        ClearCaches();

        return Ok(new CmsSaveArticleResponse(
            result.Success,
            article.Key,
            result.Success
                ? "Article updated successfully."
                : "Unable to update article."));
    }

    private CmsArticleListItemDto MapArticleListItem(
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

        return new CmsArticleListItemDto(
            content.Key,
            GetString(content, BlogContentAliases.Slug) ?? CreateSlug(title),
            title,
            GetString(content, BlogContentAliases.Summary) ?? string.Empty,
            category.Name,
            category.Name,
            category.Slug,
            GetString(content, BlogContentAliases.Level) ?? "Intermediate",
            GetString(content, BlogContentAliases.Focus) ?? "Practical",
            tags,
            GetDateTimeOffset(content, BlogContentAliases.PublishedDate),
            GetString(content, BlogContentAliases.BodyBlocks) ?? string.Empty,
            content.UpdateDate);
    }

    private void ApplyArticleValues(
        IContent article,
        CmsSaveArticleRequest request,
        IContent category)
    {
        var title = string.IsNullOrWhiteSpace(request.Title)
            ? "Untitled article"
            : request.Title.Trim();

        article.Name = title;

        article.SetValue(BlogContentAliases.Title, title);
        article.SetValue(BlogContentAliases.Slug, request.Slug?.Trim() ?? string.Empty);
        article.SetValue(BlogContentAliases.Summary, request.Summary ?? string.Empty);
        article.SetValue(BlogContentAliases.Level, request.Level ?? "Draft");
        article.SetValue(BlogContentAliases.Focus, request.Focus ?? "Preview");
        article.SetValue(BlogContentAliases.Tags, request.Tags ?? string.Empty);
        article.SetValue(BlogContentAliases.BodyBlocks, request.BodyBlocks ?? string.Empty);

        article.SetValue(
            BlogContentAliases.Category,
            $"umb://document/{category.Key:N}");

        if (!article.GetValue<DateTime?>(BlogContentAliases.PublishedDate).HasValue)
        {
            article.SetValue(BlogContentAliases.PublishedDate, DateTime.UtcNow);
        }
    }

    private int GetContentCount(string contentTypeAlias)
    {
        return _contentService
            .GetRootContent()
            .Count(content => string.Equals(
                content.ContentType.Alias,
                contentTypeAlias,
                StringComparison.OrdinalIgnoreCase));
    }

    private IReadOnlyCollection<IContent> GetRootArticles()
    {
        return _contentService
            .GetRootContent()
            .Where(content => content.ContentType.Alias == BlogContentAliases.BlogArticle)
            .ToList();
    }

    private IReadOnlyCollection<IContent> GetRootCategories()
    {
        return _contentService
            .GetRootContent()
            .Where(content => content.ContentType.Alias == BlogContentAliases.BlogCategory)
            .ToList();
    }

    private IReadOnlyDictionary<Guid, CmsCategoryDto> GetCategoryDictionary()
    {
        return GetRootCategories()
            .ToDictionary(
                content => content.Key,
                content => new CmsCategoryDto(
                    GetString(content, BlogContentAliases.Slug) ?? CreateSlug(content.Name ?? "uncategorized"),
                    GetString(content, BlogContentAliases.Title) ?? content.Name ?? "Uncategorized"));
    }

    private IContent? FindCategoryBySlug(string? categorySlug)
    {
        if (string.IsNullOrWhiteSpace(categorySlug))
        {
            return null;
        }

        return GetRootCategories()
            .FirstOrDefault(content =>
                string.Equals(
                    GetString(content, BlogContentAliases.Slug) ?? CreateSlug(content.Name ?? string.Empty),
                    categorySlug,
                    StringComparison.OrdinalIgnoreCase));
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


    private static bool IsArticleAssignedToCategory(IContent article, Guid categoryKey)
    {
        var rawCategory = GetString(article, BlogContentAliases.Category);

        if (string.IsNullOrWhiteSpace(rawCategory))
        {
            return false;
        }

        var guidText = rawCategory
            .Replace("umb://document/", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);

        return Guid.TryParse(guidText, out var resolvedCategoryKey) &&
            resolvedCategoryKey == categoryKey;
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
        return value.Trim().ToLowerInvariant().Replace(" ", "-");
    }

    private void ClearCaches()
    {
        _cache.Remove(ArticlesCacheKey);
        _cache.Remove(CategoriesCacheKey);
    }

    private sealed record CmsCategoryDto(string Slug, string Name);

    public sealed record CmsCategoryListItemDto(
        Guid Key,
        string Name,
        string Slug);

    public sealed record CmsDocumentTypeListItemDto(
        Guid Key,
        string Name,
        string Alias,
        string Icon,
        bool IsElement,
        int ContentCount);

    public sealed record CmsArticleListItemDto(
        Guid Key,
        string Slug,
        string Title,
        string Summary,
        string Category,
        string CategoryName,
        string CategorySlug,
        string Level,
        string Focus,
        IReadOnlyCollection<string> Tags,
        DateTimeOffset? PublishedDate,
        string BodyHtml,
        DateTime UpdatedUtc);

    public sealed record CmsArticleEditorDto(
        Guid Key,
        string Title,
        string Slug,
        string Summary,
        string CategoryName,
        string CategorySlug,
        string Level,
        string Focus,
        string Tags,
        string BodyBlocks,
        bool IsExistingArticle);

    public sealed record CmsSaveArticleRequest(
        string Title,
        string Slug,
        string Summary,
        string CategorySlug,
        string Tags,
        string BodyBlocks,
        string? Level,
        string? Focus);

    public sealed record CmsSaveArticleResponse(
        bool Success,
        Guid Key,
        string Message);

    public sealed record CmsDeleteResponse(
        bool Success,
        string Message);
}

