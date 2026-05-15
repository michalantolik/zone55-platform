using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using BlogPlatform.Cms.Seeding;
using BlogPlatform.Contracts.DotnetRoadmap;
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

    [HttpGet("dotnet-roadmap")]
    public ActionResult<IReadOnlyCollection<CmsDotnetZoneListItemDto>> GetDotnetRoadmap()
    {
        var articles = GetRootArticles()
            .Select(MapArticleListItem)
            .ToList();

        var zones = DotnetRoadmapCatalog.AllowedZoneKeys
            .Select((zoneKey, zoneIndex) =>
            {
                var zoneArticleCount = articles.Count(article => article.DotnetZone == zoneKey);

                var steps = DotnetRoadmapCatalog.ZoneStepKeys[zoneKey]
                    .Select((stepKey, stepIndex) => new CmsDotnetZoneStepListItemDto(
                        stepKey,
                        DotnetRoadmapCatalog.StepDisplayNames[stepKey],
                        stepIndex + 1,
                        articles.Count(article =>
                            article.DotnetZone == zoneKey &&
                            article.DotnetZoneStep == stepKey)))
                    .ToList();

                return new CmsDotnetZoneListItemDto(
                    zoneKey,
                    DotnetRoadmapCatalog.ZoneDisplayNames[zoneKey],
                    zoneIndex + 1,
                    zoneArticleCount,
                    steps);
            })
            .ToList();

        return Ok(zones);
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
        var posts = GetRootArticles()
            .Select(MapArticleListItem)
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

        return Ok(new CmsArticleEditorDto(
            article.Key,
            GetString(article, BlogContentAliases.Title) ?? article.Name ?? "Untitled",
            GetString(article, BlogContentAliases.Slug) ?? CreateSlug(article.Name ?? "article"),
            GetString(article, BlogContentAliases.Summary) ?? string.Empty,
            GetString(article, BlogContentAliases.Level) ?? "Draft",
            GetString(article, BlogContentAliases.Focus) ?? "Preview",
            GetString(article, BlogContentAliases.DotnetZone) ?? DotnetZoneKeys.Foundation,
            GetString(article, BlogContentAliases.DotnetZoneStep) ?? DotnetZoneStepKeys.BasicSyntax,
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

        if (!IsValidDotnetRoadmapAssignment(request, out var validationMessage))
        {
            return BadRequest(new CmsSaveArticleResponse(false, Guid.Empty, validationMessage));
        }

        var article = _contentService.Create(
            request.Title,
            -1,
            BlogContentAliases.BlogArticle);

        ApplyArticleValues(article, request);

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


        if (!IsValidDotnetRoadmapAssignment(request, out var validationMessage))
        {
            return BadRequest(new CmsSaveArticleResponse(false, key, validationMessage));
        }

        ApplyArticleValues(article, request);

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

    private CmsArticleListItemDto MapArticleListItem(IContent content)
    {
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
            GetString(content, BlogContentAliases.Level) ?? "Intermediate",
            GetString(content, BlogContentAliases.Focus) ?? "Practical",
            GetString(content, BlogContentAliases.DotnetZone) ?? DotnetZoneKeys.Foundation,
            GetString(content, BlogContentAliases.DotnetZoneStep) ?? DotnetZoneStepKeys.BasicSyntax,
            tags,
            GetDateTimeOffset(content, BlogContentAliases.PublishedDate),
            GetString(content, BlogContentAliases.BodyBlocks) ?? string.Empty,
            content.UpdateDate);
    }

    private void ApplyArticleValues(
        IContent article,
        CmsSaveArticleRequest request)
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
        article.SetValue(BlogContentAliases.DotnetZone, request.DotnetZone ?? DotnetZoneKeys.Foundation);
        article.SetValue(BlogContentAliases.DotnetZoneStep, request.DotnetZoneStep ?? DotnetZoneStepKeys.BasicSyntax);
        article.SetValue(BlogContentAliases.Tags, request.Tags ?? string.Empty);
        article.SetValue(BlogContentAliases.BodyBlocks, request.BodyBlocks ?? string.Empty);


        if (!article.GetValue<DateTime?>(BlogContentAliases.PublishedDate).HasValue)
        {
            article.SetValue(BlogContentAliases.PublishedDate, DateTime.UtcNow);
        }
    }

    private static bool IsValidDotnetRoadmapAssignment(
        CmsSaveArticleRequest request,
        out string validationMessage)
    {
        if (!DotnetRoadmapCatalog.IsAllowedZoneKey(request.DotnetZone))
        {
            validationMessage = $"Invalid Dotnet Zone: {request.DotnetZone}";
            return false;
        }

        if (!DotnetRoadmapCatalog.IsAllowedStepKey(request.DotnetZoneStep))
        {
            validationMessage = $"Invalid Dotnet Zone Step: {request.DotnetZoneStep}";
            return false;
        }

        if (!DotnetRoadmapCatalog.IsAllowedStepForZone(request.DotnetZone, request.DotnetZoneStep))
        {
            validationMessage = $"Dotnet Zone Step '{request.DotnetZoneStep}' does not belong to Dotnet Zone '{request.DotnetZone}'.";
            return false;
        }

        validationMessage = string.Empty;
        return true;
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

    public sealed record CmsDotnetZoneListItemDto(
        string Key,
        string Name,
        int Order,
        int ArticleCount,
        IReadOnlyCollection<CmsDotnetZoneStepListItemDto> Steps);

    public sealed record CmsDotnetZoneStepListItemDto(
        string Key,
        string Name,
        int Order,
        int ArticleCount);

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
        string Level,
        string Focus,
        string? DotnetZone,
        string? DotnetZoneStep,
        IReadOnlyCollection<string> Tags,
        DateTimeOffset? PublishedDate,
        string BodyHtml,
        DateTime UpdatedUtc);

    public sealed record CmsArticleEditorDto(
        Guid Key,
        string Title,
        string Slug,
        string Summary,
        string Level,
        string Focus,
        string? DotnetZone,
        string? DotnetZoneStep,
        string Tags,
        string BodyBlocks,
        bool IsExistingArticle);

    public sealed record CmsSaveArticleRequest(
        string Title,
        string Slug,
        string Summary,
        string Tags,
        string BodyBlocks,
        string? Level,
        string? Focus,
        string? DotnetZone,
        string? DotnetZoneStep);

    public sealed record CmsSaveArticleResponse(
        bool Success,
        Guid Key,
        string Message);

    public sealed record CmsDeleteResponse(
        bool Success,
        string Message);
}
