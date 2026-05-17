using BlogPlatform.Application.Roadmap;
using BlogPlatform.Cms.Seeding;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace BlogPlatform.Cms.BlogContent;

public sealed class BlogContentAdminService : IBlogContentAdminService
{
    private const string ArticlesCacheKey = "cms-blog-articles";
    private const string DefaultDotnetZone = "foundation";
    private const string DefaultDotnetZoneStep = "basic-syntax";

    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<BlogContentAdminService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IRoadmapQueryService _roadmapQueries;
    private readonly IRoadmapCommandService _roadmapCommands;

    public BlogContentAdminService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        ILogger<BlogContentAdminService> logger,
        IMemoryCache cache,
        IRoadmapQueryService roadmapQueries,
        IRoadmapCommandService roadmapCommands)
    {
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _logger = logger;
        _cache = cache;
        _roadmapQueries = roadmapQueries;
        _roadmapCommands = roadmapCommands;
    }

    public async Task<IReadOnlyCollection<CmsDotnetZoneListItemDto>> GetDotnetRoadmapAsync(
        CancellationToken cancellationToken)
    {
        var articles = GetRootArticles()
            .Select(MapArticleListItem)
            .ToList();

        var roadmap = await _roadmapQueries.GetRoadmapAsync(cancellationToken);

        return roadmap
            .OrderBy(zone => zone.Order)
            .Select(zone =>
            {
                var zoneArticleCount = articles.Count(article =>
                    article.DotnetZone == zone.Key);

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
    }

    public IReadOnlyCollection<CmsDocumentTypeListItemDto> GetDocumentTypes()
    {
        return _contentTypeService
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
    }

    public IReadOnlyCollection<CmsArticleListItemDto> GetArticles()
    {
        return GetRootArticles()
            .Select(MapArticleListItem)
            .OrderByDescending(article => article.PublishedDate)
            .ThenBy(article => article.Title)
            .ToList();
    }

    public CmsArticleEditorDto? GetArticle(Guid key)
    {
        var article = _contentService.GetById(key);

        if (article is null ||
            article.ContentType.Alias != BlogContentAliases.BlogArticle)
        {
            return null;
        }

        return new CmsArticleEditorDto(
            article.Key,
            GetString(article, BlogContentAliases.Title) ?? article.Name ?? "Untitled",
            GetString(article, BlogContentAliases.Slug) ?? CreateSlug(article.Name ?? "article"),
            GetString(article, BlogContentAliases.Summary) ?? string.Empty,
            GetString(article, BlogContentAliases.Level) ?? "Draft",
            GetString(article, BlogContentAliases.Focus) ?? "Preview",
            GetString(article, BlogContentAliases.DotnetZone) ?? DefaultDotnetZone,
            GetString(article, BlogContentAliases.DotnetZoneStep) ?? DefaultDotnetZoneStep,
            GetInt(article, BlogContentAliases.Order),
            GetString(article, BlogContentAliases.Tags) ?? string.Empty,
            GetString(article, BlogContentAliases.BodyBlocks) ?? string.Empty,
            true);
    }

    public int GetNextArticleOrder(
        string? dotnetZone,
        string? dotnetZoneStep,
        Guid? excludeArticleKey = null)
    {
        var normalizedZone = NormalizeAssignmentKey(dotnetZone) ?? DefaultDotnetZone;
        var normalizedStep = NormalizeAssignmentKey(dotnetZoneStep) ?? DefaultDotnetZoneStep;

        var highestExistingOrder = GetRootArticles()
            .Where(article => !excludeArticleKey.HasValue || article.Key != excludeArticleKey.Value)
            .Where(article => string.Equals(
                GetString(article, BlogContentAliases.DotnetZone) ?? DefaultDotnetZone,
                normalizedZone,
                StringComparison.OrdinalIgnoreCase))
            .Where(article => string.Equals(
                GetString(article, BlogContentAliases.DotnetZoneStep) ?? DefaultDotnetZoneStep,
                normalizedStep,
                StringComparison.OrdinalIgnoreCase))
            .Select(article => GetInt(article, BlogContentAliases.Order))
            .Where(order => order > 0)
            .DefaultIfEmpty(0)
            .Max();

        return highestExistingOrder + 1;
    }

    public async Task<CmsSaveArticleResponse> CreateArticleAsync(
        CmsSaveArticleRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateDotnetRoadmapAssignmentAsync(
            request,
            cancellationToken);

        if (!validation.Success)
        {
            return new CmsSaveArticleResponse(false, Guid.Empty, validation.Message);
        }

        var article = _contentService.Create(
            request.Title,
            -1,
            BlogContentAliases.BlogArticle);

        ApplyArticleValues(article, request, GetArticleOrderForCreate(request));

        _contentService.Save(article);
        var result = _contentService.Publish(article, ["*"]);

        ClearCaches();

        return new CmsSaveArticleResponse(
            result.Success,
            article.Key,
            result.Success
                ? "Article created successfully."
                : "Unable to create article.");
    }

    public async Task<CmsSaveArticleResponse> UpdateArticleAsync(
        Guid key,
        CmsSaveArticleRequest request,
        CancellationToken cancellationToken)
    {
        var article = _contentService.GetById(key);

        if (article is null ||
            article.ContentType.Alias != BlogContentAliases.BlogArticle)
        {
            return new CmsSaveArticleResponse(false, key, "Article not found.");
        }

        var validation = await ValidateDotnetRoadmapAssignmentAsync(
            request,
            cancellationToken);

        if (!validation.Success)
        {
            return new CmsSaveArticleResponse(false, key, validation.Message);
        }

        ApplyArticleValues(article, request, GetArticleOrderForUpdate(article, request));

        _contentService.Save(article);
        var result = _contentService.Publish(article, ["*"]);

        ClearCaches();

        return new CmsSaveArticleResponse(
            result.Success,
            article.Key,
            result.Success
                ? "Article updated successfully."
                : "Unable to update article.");
    }

    public CmsDeleteResponse DeleteArticle(Guid key)
    {
        var article = _contentService.GetById(key);

        if (article is null ||
            article.ContentType.Alias != BlogContentAliases.BlogArticle)
        {
            return new CmsDeleteResponse(false, "Article not found.");
        }

        try
        {
            _contentService.Delete(article);
            ClearCaches();

            return new CmsDeleteResponse(true, "Article deleted successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to delete CMS article. Key={ArticleKey}",
                key);

            return new CmsDeleteResponse(false, "Unable to delete article.");
        }
    }

    public CmsDeleteResponse DeleteDocumentType(Guid key)
    {
        var documentType = _contentTypeService.Get(key);

        if (documentType is null)
        {
            return new CmsDeleteResponse(false, "Document type not found.");
        }

        var contentCount = GetContentCount(documentType.Alias);

        if (contentCount > 0)
        {
            return new CmsDeleteResponse(
                false,
                $"Cannot delete document type because {contentCount} content item{(contentCount == 1 ? string.Empty : "s")} still use it.");
        }

        try
        {
            _contentTypeService.Delete(documentType);
            ClearCaches();

            return new CmsDeleteResponse(true, "Document type deleted successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to delete CMS document type. Key={DocumentTypeKey}",
                key);

            return new CmsDeleteResponse(false, "Unable to delete document type.");
        }
    }

    public async Task<CmsSaveRoadmapResponse> CreateZoneAsync(
        CmsSaveRoadmapZoneRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roadmapCommands.CreateZoneAsync(
            request.Name,
            request.Key,
            cancellationToken);

        ClearCachesIfSuccessful(result);

        return new CmsSaveRoadmapResponse(result.Success, result.Message);
    }

    public async Task<CmsSaveRoadmapResponse> UpdateZoneAsync(
        string zoneKey,
        CmsSaveRoadmapZoneRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roadmapCommands.UpdateZoneAsync(
            zoneKey,
            request.Name,
            cancellationToken);

        ClearCachesIfSuccessful(result);

        return new CmsSaveRoadmapResponse(result.Success, result.Message);
    }

    public async Task<CmsDeleteResponse> DeleteZoneAsync(
        string zoneKey,
        CancellationToken cancellationToken)
    {
        var result = await _roadmapCommands.DeleteZoneAsync(
            zoneKey,
            cancellationToken);

        ClearCachesIfSuccessful(result);

        return new CmsDeleteResponse(result.Success, result.Message);
    }

    public async Task<CmsSaveRoadmapResponse> CreateStepAsync(
        string zoneKey,
        CmsSaveRoadmapStepRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roadmapCommands.CreateStepAsync(
            zoneKey,
            request.Name,
            request.Key,
            cancellationToken);

        ClearCachesIfSuccessful(result);

        return new CmsSaveRoadmapResponse(result.Success, result.Message);
    }

    public async Task<CmsSaveRoadmapResponse> UpdateStepAsync(
        string zoneKey,
        string stepKey,
        CmsSaveRoadmapStepRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roadmapCommands.UpdateStepAsync(
            zoneKey,
            stepKey,
            request.Name,
            cancellationToken);

        ClearCachesIfSuccessful(result);

        return new CmsSaveRoadmapResponse(result.Success, result.Message);
    }

    public async Task<CmsDeleteResponse> DeleteStepAsync(
        string zoneKey,
        string stepKey,
        CancellationToken cancellationToken)
    {
        var result = await _roadmapCommands.DeleteStepAsync(
            zoneKey,
            stepKey,
            cancellationToken);

        ClearCachesIfSuccessful(result);

        return new CmsDeleteResponse(result.Success, result.Message);
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
            GetString(content, BlogContentAliases.DotnetZone) ?? DefaultDotnetZone,
            GetString(content, BlogContentAliases.DotnetZoneStep) ?? DefaultDotnetZoneStep,
            GetInt(content, BlogContentAliases.Order),
            tags,
            GetDateTimeOffset(content, BlogContentAliases.PublishedDate),
            GetString(content, BlogContentAliases.BodyBlocks) ?? string.Empty,
            content.UpdateDate);
    }

    private void ApplyArticleValues(
        IContent article,
        CmsSaveArticleRequest request,
        int order)
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
        article.SetValue(BlogContentAliases.DotnetZone, request.DotnetZone ?? DefaultDotnetZone);
        article.SetValue(BlogContentAliases.DotnetZoneStep, request.DotnetZoneStep ?? DefaultDotnetZoneStep);
        article.SetValue(BlogContentAliases.Order, order);
        article.SetValue(BlogContentAliases.Tags, request.Tags ?? string.Empty);
        article.SetValue(BlogContentAliases.BodyBlocks, request.BodyBlocks ?? string.Empty);

        if (!article.GetValue<DateTime?>(BlogContentAliases.PublishedDate).HasValue)
        {
            article.SetValue(BlogContentAliases.PublishedDate, DateTime.UtcNow);
        }
    }

    private int GetArticleOrderForCreate(CmsSaveArticleRequest request)
    {
        if (request.Order.HasValue && request.Order.Value > 0)
        {
            return request.Order.Value;
        }

        return GetNextArticleOrder(
            request.DotnetZone,
            request.DotnetZoneStep);
    }

    private int GetArticleOrderForUpdate(IContent article, CmsSaveArticleRequest request)
    {
        var currentZone = GetString(article, BlogContentAliases.DotnetZone) ?? DefaultDotnetZone;
        var currentStep = GetString(article, BlogContentAliases.DotnetZoneStep) ?? DefaultDotnetZoneStep;
        var requestedZone = NormalizeAssignmentKey(request.DotnetZone) ?? DefaultDotnetZone;
        var requestedStep = NormalizeAssignmentKey(request.DotnetZoneStep) ?? DefaultDotnetZoneStep;
        var currentOrder = GetInt(article, BlogContentAliases.Order);

        var assignmentChanged =
            !string.Equals(currentZone, requestedZone, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(currentStep, requestedStep, StringComparison.OrdinalIgnoreCase);

        if (assignmentChanged)
        {
            return GetNextArticleOrder(
                requestedZone,
                requestedStep,
                article.Key);
        }

        if (request.Order.HasValue && request.Order.Value > 0)
        {
            return request.Order.Value;
        }

        if (currentOrder > 0)
        {
            return currentOrder;
        }

        return GetNextArticleOrder(
            requestedZone,
            requestedStep,
            article.Key);
    }

    private Task<RoadmapOperationResult> ValidateDotnetRoadmapAssignmentAsync(
        CmsSaveArticleRequest request,
        CancellationToken cancellationToken)
    {
        return _roadmapQueries.ValidateStepAsync(
            request.DotnetZone,
            request.DotnetZoneStep,
            cancellationToken);
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

    private static string? GetString(IContent content, string alias)
    {
        return content.GetValue<string>(alias);
    }

    private static int GetInt(IContent content, string alias)
    {
        return content.GetValue<int?>(alias) ?? 0;
    }

    private static string? NormalizeAssignmentKey(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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

    private void ClearCachesIfSuccessful(RoadmapOperationResult result)
    {
        if (result.Success)
        {
            ClearCaches();
        }
    }

    private void ClearCaches()
    {
        _cache.Remove(ArticlesCacheKey);
    }
}
