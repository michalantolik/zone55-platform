using BlogPlatform.Application.Roadmap;
using BlogPlatform.Cms.Seeding;
using BlogPlatform.Contracts.DotnetRoadmap;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
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
    private readonly IDotnetRoadmapStore _roadmapStore;

    public BlogContentController(
        IContentService contentService,
        IContentTypeService contentTypeService,
        ILogger<BlogContentController> logger,
        IMemoryCache cache,
        IDotnetRoadmapStore roadmapStore)
    {
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _logger = logger;
        _cache = cache;
        _roadmapStore = roadmapStore;
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
    public async Task<ActionResult<IReadOnlyCollection<CmsDotnetZoneListItemDto>>> GetDotnetRoadmap()
    {
        var articles = GetRootArticles()
            .Select(MapArticleListItem)
            .ToList();

        var roadmap = await _roadmapStore.GetAsync();

        var zones = roadmap.Zones
            .OrderBy(zone => zone.Order)
            .Select(zone =>
            {
                var zoneArticleCount = articles.Count(article => article.DotnetZone == zone.Key);

                var steps = zone.Steps
                    .OrderBy(step => step.Order)
                    .Select(step => new CmsDotnetZoneStepListItemDto(
                        step.Key,
                        step.Name,
                        step.Order,
                        articles.Count(article =>
                            article.DotnetZone == zone.Key &&
                            article.DotnetZoneStep == step.Key)))
                    .ToList();

                return new CmsDotnetZoneListItemDto(
                    zone.Key,
                    zone.Name,
                    zone.Order,
                    zoneArticleCount,
                    steps);
            })
            .ToList();

        return Ok(zones);
    }

    [HttpPost("dotnet-roadmap/zones")]
    public async Task<ActionResult<CmsSaveRoadmapResponse>> CreateZone(
        [FromBody] CmsSaveRoadmapZoneRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new CmsSaveRoadmapResponse(false, "Zone name is required."));
        }

        var roadmap = await _roadmapStore.GetAsync();
        var key = CreateSlug(string.IsNullOrWhiteSpace(request.Key) ? request.Name : request.Key);

        if (roadmap.Zones.Any(zone => zone.Key == key))
        {
            return BadRequest(new CmsSaveRoadmapResponse(false, "Zone key already exists."));
        }

        roadmap.Zones.Add(new DotnetRoadmapZone
        {
            Key = key,
            Name = request.Name.Trim(),
            Order = roadmap.Zones.Count + 1,
            Steps = []
        });

        await _roadmapStore.SaveAsync(roadmap);
        ClearCaches();

        return Ok(new CmsSaveRoadmapResponse(true, "Zone created successfully."));
    }

    [HttpPut("dotnet-roadmap/zones/{zoneKey}")]
    public async Task<ActionResult<CmsSaveRoadmapResponse>> UpdateZone(
        string zoneKey,
        [FromBody] CmsSaveRoadmapZoneRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new CmsSaveRoadmapResponse(false, "Zone name is required."));
        }

        var roadmap = await _roadmapStore.GetAsync();
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == zoneKey);

        if (zone is null)
        {
            return NotFound(new CmsSaveRoadmapResponse(false, "Zone not found."));
        }

        zone.Name = request.Name.Trim();

        await _roadmapStore.SaveAsync(roadmap);
        ClearCaches();

        return Ok(new CmsSaveRoadmapResponse(true, "Zone updated successfully."));
    }

    [HttpDelete("dotnet-roadmap/zones/{zoneKey}")]
    public async Task<ActionResult<CmsDeleteResponse>> DeleteZone(string zoneKey)
    {
        var articlesUsingZone = GetRootArticles()
            .Select(MapArticleListItem)
            .Count(article => article.DotnetZone == zoneKey);

        if (articlesUsingZone > 0)
        {
            return BadRequest(new CmsDeleteResponse(false, "Cannot delete zone because articles still use it."));
        }

        var roadmap = await _roadmapStore.GetAsync();
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == zoneKey);

        if (zone is null)
        {
            return NotFound(new CmsDeleteResponse(false, "Zone not found."));
        }

        roadmap.Zones.Remove(zone);
        ReorderZones(roadmap);

        await _roadmapStore.SaveAsync(roadmap);
        ClearCaches();

        return Ok(new CmsDeleteResponse(true, "Zone deleted successfully."));
    }

    [HttpPost("dotnet-roadmap/zones/{zoneKey}/steps")]
    public async Task<ActionResult<CmsSaveRoadmapResponse>> CreateStep(
        string zoneKey,
        [FromBody] CmsSaveRoadmapStepRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new CmsSaveRoadmapResponse(false, "Step name is required."));
        }

        var roadmap = await _roadmapStore.GetAsync();
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == zoneKey);

        if (zone is null)
        {
            return NotFound(new CmsSaveRoadmapResponse(false, "Zone not found."));
        }

        var key = CreateSlug(string.IsNullOrWhiteSpace(request.Key) ? request.Name : request.Key);

        if (roadmap.Zones.SelectMany(item => item.Steps).Any(step => step.Key == key))
        {
            return BadRequest(new CmsSaveRoadmapResponse(false, "Step key already exists."));
        }

        zone.Steps.Add(new DotnetRoadmapStep
        {
            Key = key,
            Name = request.Name.Trim(),
            Order = zone.Steps.Count + 1
        });

        await _roadmapStore.SaveAsync(roadmap);
        ClearCaches();

        return Ok(new CmsSaveRoadmapResponse(true, "Step created successfully."));
    }

    [HttpPut("dotnet-roadmap/zones/{zoneKey}/steps/{stepKey}")]
    public async Task<ActionResult<CmsSaveRoadmapResponse>> UpdateStep(
        string zoneKey,
        string stepKey,
        [FromBody] CmsSaveRoadmapStepRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new CmsSaveRoadmapResponse(false, "Step name is required."));
        }

        var roadmap = await _roadmapStore.GetAsync();
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == zoneKey);
        var step = zone?.Steps.FirstOrDefault(item => item.Key == stepKey);

        if (zone is null || step is null)
        {
            return NotFound(new CmsSaveRoadmapResponse(false, "Step not found."));
        }

        step.Name = request.Name.Trim();

        await _roadmapStore.SaveAsync(roadmap);
        ClearCaches();

        return Ok(new CmsSaveRoadmapResponse(true, "Step updated successfully."));
    }

    [HttpDelete("dotnet-roadmap/zones/{zoneKey}/steps/{stepKey}")]
    public async Task<ActionResult<CmsDeleteResponse>> DeleteStep(
        string zoneKey,
        string stepKey)
    {
        var articlesUsingStep = GetRootArticles()
            .Select(MapArticleListItem)
            .Count(article => article.DotnetZone == zoneKey && article.DotnetZoneStep == stepKey);

        if (articlesUsingStep > 0)
        {
            return BadRequest(new CmsDeleteResponse(false, "Cannot delete step because articles still use it."));
        }

        var roadmap = await _roadmapStore.GetAsync();
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == zoneKey);
        var step = zone?.Steps.FirstOrDefault(item => item.Key == stepKey);

        if (zone is null || step is null)
        {
            return NotFound(new CmsDeleteResponse(false, "Step not found."));
        }

        zone.Steps.Remove(step);
        ReorderSteps(zone);

        await _roadmapStore.SaveAsync(roadmap);
        ClearCaches();

        return Ok(new CmsDeleteResponse(true, "Step deleted successfully."));
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
    public async Task<ActionResult<CmsSaveArticleResponse>> CreateArticle(
        [FromBody] CmsSaveArticleRequest request)
    {

        var roadmapValidation = await ValidateDotnetRoadmapAssignmentAsync(request);

        if (!roadmapValidation.IsValid)
        {
            return BadRequest(new CmsSaveArticleResponse(false, Guid.Empty, roadmapValidation.Message));
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
    public async Task<ActionResult<CmsSaveArticleResponse>> UpdateArticle(
        Guid key,
        [FromBody] CmsSaveArticleRequest request)
    {
        var article = _contentService.GetById(key);

        if (article is null ||
            article.ContentType.Alias != BlogContentAliases.BlogArticle)
        {
            return NotFound();
        }


        var roadmapValidation = await ValidateDotnetRoadmapAssignmentAsync(request);

        if (!roadmapValidation.IsValid)
        {
            return BadRequest(new CmsSaveArticleResponse(false, key, roadmapValidation.Message));
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

    private async Task<RoadmapValidationResult> ValidateDotnetRoadmapAssignmentAsync(
        CmsSaveArticleRequest request)
    {
        var roadmap = await _roadmapStore.GetAsync();
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == request.DotnetZone);

        if (zone is null)
        {
            return new RoadmapValidationResult(
                false,
                $"Invalid Dotnet Zone: {request.DotnetZone}");
        }

        if (zone.Steps.All(item => item.Key != request.DotnetZoneStep))
        {
            return new RoadmapValidationResult(
                false,
                $"Dotnet Zone Step '{request.DotnetZoneStep}' does not belong to Dotnet Zone '{request.DotnetZone}'.");
        }

        return new RoadmapValidationResult(true, string.Empty);
    }

    private static void ReorderZones(DotnetRoadmap roadmap)
    {
        var index = 1;

        foreach (var zone in roadmap.Zones.OrderBy(zone => zone.Order))
        {
            zone.Order = index++;
        }
    }

    private static void ReorderSteps(DotnetRoadmapZone zone)
    {
        var index = 1;

        foreach (var step in zone.Steps.OrderBy(step => step.Order))
        {
            step.Order = index++;
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
        var normalized = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder();
        var lastWasDash = false;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                lastWasDash = false;
                continue;
            }

            if (!lastWasDash)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        return builder.ToString().Trim('-');
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

    public sealed record CmsSaveRoadmapZoneRequest(
        string Name,
        string? Key);

    public sealed record CmsSaveRoadmapStepRequest(
        string Name,
        string? Key);

    public sealed record CmsSaveRoadmapResponse(
        bool Success,
        string Message);

    private sealed record RoadmapValidationResult(
        bool IsValid,
        string Message);

    public sealed record CmsDeleteResponse(
        bool Success,
        string Message);
}
