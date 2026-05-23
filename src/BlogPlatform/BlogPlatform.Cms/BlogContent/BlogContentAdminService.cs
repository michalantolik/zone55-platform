using BlogPlatform.Application.Roadmap;
using BlogPlatform.Cms.Seeding;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
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
                        step.Icon,
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

    public async Task<CmsDatabaseSummaryDto> GetDatabaseSummaryAsync(
        CancellationToken cancellationToken)
    {
        var roadmap = await _roadmapQueries.GetRoadmapAsync(cancellationToken);

        return new CmsDatabaseSummaryDto(
            roadmap.Count,
            roadmap.Sum(zone => zone.Steps.Count),
            GetRootArticles().Count);
    }

    public async Task<BlogSeedContent> BuildSeedContentAsync(
        CancellationToken cancellationToken)
    {
        var roadmap = await _roadmapQueries.GetRoadmapAsync(cancellationToken);

        return new BlogSeedContent
        {
            RoadmapZones = roadmap
                .OrderBy(zone => zone.Order)
                .Select(zone => new BlogSeedRoadmapZone
                {
                    Key = zone.Key,
                    Name = zone.Name,
                    Order = zone.Order,
                    Steps = zone.Steps
                        .OrderBy(step => step.Order)
                        .Select(step => new BlogSeedRoadmapStep
                        {
                            Key = step.Key,
                            Name = step.Name,
                            Order = step.Order,
                            Icon = string.IsNullOrWhiteSpace(step.Icon)
                                ? "📘"
                                : step.Icon
                        })
                        .ToList()
                })
                .ToList(),

            Articles = GetRootArticles()
                .Select(article =>
                {
                    var title = GetString(article, BlogContentAliases.Title)
                        ?? article.Name
                        ?? "Untitled";

                    return new BlogSeedArticle
                    {
                        Name = title,
                        Slug = GetString(article, BlogContentAliases.Slug) ?? CreateSlug(title),
                        Level = GetString(article, BlogContentAliases.Level) ?? "Intermediate",
                        Focus = GetString(article, BlogContentAliases.Focus) ?? "Practical",
                        Summary = GetString(article, BlogContentAliases.Summary) ?? string.Empty,
                        DotnetZone = GetString(article, BlogContentAliases.DotnetZone) ?? DefaultDotnetZone,
                        DotnetZoneStep = GetString(article, BlogContentAliases.DotnetZoneStep) ?? DefaultDotnetZoneStep,
                        Order = GetInt(article, BlogContentAliases.Order),
                        Tags = GetSeedTags(article),
                        BodyBlocks = GetSeedBodyBlocks(article)
                    };
                })
                .OrderBy(article => article.DotnetZone)
                .ThenBy(article => article.DotnetZoneStep)
                .ThenBy(article => article.Order <= 0 ? int.MaxValue : article.Order)
                .ThenBy(article => article.Name)
                .ToList()
        };
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
        return _cache.GetOrCreate(
            ArticlesCacheKey,
            entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                return GetRootArticles()
                    .Select(MapArticleListItem)
                    .OrderBy(article => article.DotnetZone)
                    .ThenBy(article => article.DotnetZoneStep)
                    .ThenBy(article => article.Order <= 0 ? int.MaxValue : article.Order)
                    .ThenByDescending(article => article.UpdatedUtc)
                    .ToList();
            }) ?? [];
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
            GetString(article, BlogContentAliases.Title) ?? article.Name ?? string.Empty,
            GetString(article, BlogContentAliases.Slug) ?? string.Empty,
            GetString(article, BlogContentAliases.Summary) ?? string.Empty,
            GetString(article, BlogContentAliases.Level) ?? string.Empty,
            GetString(article, BlogContentAliases.Focus) ?? string.Empty,
            GetString(article, BlogContentAliases.DotnetZone),
            GetString(article, BlogContentAliases.DotnetZoneStep),
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
        var normalizedZone = string.IsNullOrWhiteSpace(dotnetZone)
            ? DefaultDotnetZone
            : dotnetZone;

        var normalizedStep = string.IsNullOrWhiteSpace(dotnetZoneStep)
            ? DefaultDotnetZoneStep
            : dotnetZoneStep;

        return GetRootArticles()
            .Where(article => !excludeArticleKey.HasValue || article.Key != excludeArticleKey.Value)
            .Where(article => string.Equals(
                GetString(article, BlogContentAliases.DotnetZone),
                normalizedZone,
                StringComparison.OrdinalIgnoreCase))
            .Where(article => string.Equals(
                GetString(article, BlogContentAliases.DotnetZoneStep),
                normalizedStep,
                StringComparison.OrdinalIgnoreCase))
            .Select(article => GetInt(article, BlogContentAliases.Order))
            .Where(order => order > 0)
            .DefaultIfEmpty(0)
            .Max() + 1;
    }

    public IReadOnlyCollection<CmsReorderArticleListItemDto> GetArticlesForReorder(
        string? dotnetZone,
        string? dotnetZoneStep)
    {
        var normalizedZone = string.IsNullOrWhiteSpace(dotnetZone)
            ? DefaultDotnetZone
            : dotnetZone;

        var normalizedStep = string.IsNullOrWhiteSpace(dotnetZoneStep)
            ? DefaultDotnetZoneStep
            : dotnetZoneStep;

        return GetRootArticles()
            .Where(article => string.Equals(
                GetString(article, BlogContentAliases.DotnetZone),
                normalizedZone,
                StringComparison.OrdinalIgnoreCase))
            .Where(article => string.Equals(
                GetString(article, BlogContentAliases.DotnetZoneStep),
                normalizedStep,
                StringComparison.OrdinalIgnoreCase))
            .Select(MapReorderArticleListItem)
            .OrderBy(article => article.Order <= 0 ? int.MaxValue : article.Order)
            .ThenBy(article => article.Title)
            .ToList();
    }

    public CmsReorderArticlesResponse ReorderArticles(CmsReorderArticlesRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DotnetZone) ||
            string.IsNullOrWhiteSpace(request.DotnetZoneStep))
        {
            return new CmsReorderArticlesResponse(
                false,
                "Zone and step are required.",
                []);
        }

        var articleKeys = request.ArticleKeys.ToList();

        for (var index = 0; index < articleKeys.Count; index++)
        {
            var article = _contentService.GetById(articleKeys[index]);

            if (article is null ||
                article.ContentType.Alias != BlogContentAliases.BlogArticle)
            {
                continue;
            }

            article.SetValue(BlogContentAliases.DotnetZone, request.DotnetZone);
            article.SetValue(BlogContentAliases.DotnetZoneStep, request.DotnetZoneStep);
            article.SetValue(BlogContentAliases.Order, index + 1);

            _contentService.Save(article);
            _contentService.Publish(article, new[] { "*" });
        }

        ClearCaches();

        return new CmsReorderArticlesResponse(
            true,
            "Article order saved.",
            GetArticlesForReorder(request.DotnetZone, request.DotnetZoneStep));
    }

    public async Task<CmsSaveArticleResponse> CreateArticleAsync(
        CmsSaveArticleRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateArticleRequest(request);

        if (validation is not null)
        {
            return validation;
        }

        var slug = CreateSlug(request.Slug);
        var existingArticle = GetRootArticles()
            .FirstOrDefault(article => string.Equals(
                GetString(article, BlogContentAliases.Slug),
                slug,
                StringComparison.OrdinalIgnoreCase));

        if (existingArticle is not null)
        {
            return new CmsSaveArticleResponse(
                false,
                Guid.Empty,
                "Article slug already exists.");
        }

        var article = _contentService.Create(
            request.Title.Trim(),
            -1,
            BlogContentAliases.BlogArticle);

        ApplyArticleValues(article, request, slug);
        _contentService.Save(article);
        _contentService.Publish(article, new[] { "*" });

        ClearCaches();

        await Task.CompletedTask;

        return new CmsSaveArticleResponse(
            true,
            article.Key,
            "Article created.");
    }

    public async Task<CmsSaveArticleResponse> UpdateArticleAsync(
        Guid key,
        CmsSaveArticleRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateArticleRequest(request);

        if (validation is not null)
        {
            return validation;
        }

        var article = _contentService.GetById(key);

        if (article is null ||
            article.ContentType.Alias != BlogContentAliases.BlogArticle)
        {
            return new CmsSaveArticleResponse(
                false,
                key,
                "Article not found.");
        }

        var slug = CreateSlug(request.Slug);
        var existingArticle = GetRootArticles()
            .FirstOrDefault(existing =>
                existing.Key != key &&
                string.Equals(
                    GetString(existing, BlogContentAliases.Slug),
                    slug,
                    StringComparison.OrdinalIgnoreCase));

        if (existingArticle is not null)
        {
            return new CmsSaveArticleResponse(
                false,
                key,
                "Article slug already exists.");
        }

        article.Name = request.Title.Trim();
        ApplyArticleValues(article, request, slug);

        _contentService.Save(article);
        _contentService.Publish(article, new[] { "*" });

        ClearCaches();

        await Task.CompletedTask;

        return new CmsSaveArticleResponse(
            true,
            article.Key,
            "Article updated.");
    }

    public CmsDeleteResponse DeleteArticle(Guid key)
    {
        var article = _contentService.GetById(key);

        if (article is null ||
            article.ContentType.Alias != BlogContentAliases.BlogArticle)
        {
            return new CmsDeleteResponse(false, "Article not found.");
        }

        _contentService.Delete(article);
        ClearCaches();

        return new CmsDeleteResponse(true, "Article deleted.");
    }

    public CmsDeleteResponse DeleteDocumentType(Guid key)
    {
        var documentType = _contentTypeService.Get(key);

        if (documentType is null)
        {
            return new CmsDeleteResponse(false, "Document type not found.");
        }

        if (GetContentCount(documentType.Alias) > 0)
        {
            return new CmsDeleteResponse(
                false,
                "Document type cannot be deleted while content exists.");
        }

        _contentTypeService.Delete(documentType, -1);

        return new CmsDeleteResponse(true, "Document type deleted.");
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
            request.Icon,
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
            request.Icon,
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

    private static List<string> GetSeedTags(IContent content)
    {
        return GetString(content, BlogContentAliases.Tags)?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToList() ?? [];
    }

    private List<BlogSeedBlock> GetSeedBodyBlocks(IContent content)
    {
        var bodyBlocksJson = GetString(content, BlogContentAliases.BodyBlocks);

        if (string.IsNullOrWhiteSpace(bodyBlocksJson))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(bodyBlocksJson);
            var root = document.RootElement;

            if (!root.TryGetProperty("contentData", out var contentData) ||
                contentData.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var blocks = new List<BlogSeedBlock>();

            foreach (var blockElement in contentData.EnumerateArray())
            {
                var block = CreateSeedBlockFromBlockListContent(blockElement);

                if (block is not null)
                {
                    blocks.Add(block);
                }
            }

            return blocks;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not export body blocks for article {ArticleName}. Invalid Block List JSON was found.",
                content.Name);

            return [];
        }
    }

    private BlogSeedBlock? CreateSeedBlockFromBlockListContent(JsonElement blockElement)
    {
        var elementTypeAlias = GetElementTypeAlias(blockElement);

        return elementTypeAlias switch
        {
            BlogContentAliases.TextBlock => new BlogSeedBlock
            {
                Type = "text",
                Text = GetJsonString(blockElement, "text")
            },

            BlogContentAliases.HeadingBlock => new BlogSeedBlock
            {
                Type = "heading",
                Level = GetJsonInt(blockElement, "level"),
                Text = GetJsonString(blockElement, "text")
            },

            BlogContentAliases.CodeSnippetBlock => new BlogSeedBlock
            {
                Type = "codeSnippet",
                Language = GetJsonString(blockElement, "language"),
                FileName = GetJsonString(blockElement, "fileName"),
                Code = GetJsonString(blockElement, "code")
            },

            BlogContentAliases.MermaidDiagramBlock => new BlogSeedBlock
            {
                Type = "mermaidDiagram",
                Title = GetJsonString(blockElement, "title"),
                ShowDiagramTitleBar = GetJsonBool(blockElement, "showDiagramTitleBar"),
                Diagram = GetJsonString(blockElement, "diagram")
            },

            BlogContentAliases.PlantUmlDiagramBlock => new BlogSeedBlock
            {
                Type = "plantUmlDiagram",
                Title = GetJsonString(blockElement, "title"),
                ShowDiagramTitleBar = GetJsonBool(blockElement, "showDiagramTitleBar"),
                Diagram = GetJsonString(blockElement, "diagram")
            },

            BlogContentAliases.CalloutBlock => new BlogSeedBlock
            {
                Type = "callout",
                Kind = GetJsonString(blockElement, "kind"),
                Text = GetJsonString(blockElement, "text")
            },

            BlogContentAliases.SummaryBlock => new BlogSeedBlock
            {
                Type = "summary",
                Summary = GetJsonString(blockElement, "summary")
            },

            _ => null
        };
    }

    private string? GetElementTypeAlias(JsonElement blockElement)
    {
        var contentTypeKeyValue = GetJsonString(blockElement, "contentTypeKey");

        if (string.IsNullOrWhiteSpace(contentTypeKeyValue) ||
            !Guid.TryParse(contentTypeKeyValue, out var contentTypeKey))
        {
            return null;
        }

        return _contentTypeService.Get(contentTypeKey)?.Alias;
    }

    private static string? GetJsonString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    private static int? GetJsonInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number &&
            property.TryGetInt32(out var value))
        {
            return value;
        }

        if (property.ValueKind == JsonValueKind.String &&
            int.TryParse(property.GetString(), out value))
        {
            return value;
        }

        return null;
    }

    private static bool? GetJsonBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(property.GetString(), out var value) => value,
            _ => null
        };
    }

    private CmsArticleListItemDto MapArticleListItem(IContent content)
    {
        var title = GetString(content, BlogContentAliases.Title)
            ?? content.Name
            ?? "Untitled";

        var slug = GetString(content, BlogContentAliases.Slug) ?? CreateSlug(title);
        var summary = GetString(content, BlogContentAliases.Summary) ?? string.Empty;
        var level = GetString(content, BlogContentAliases.Level) ?? string.Empty;
        var focus = GetString(content, BlogContentAliases.Focus) ?? string.Empty;
        var dotnetZone = GetString(content, BlogContentAliases.DotnetZone);
        var dotnetZoneStep = GetString(content, BlogContentAliases.DotnetZoneStep);
        var order = GetInt(content, BlogContentAliases.Order);
        var tags = GetSeedTags(content);
        var publishedDate = GetDateTimeOffset(content, BlogContentAliases.PublishedDate);

        return new CmsArticleListItemDto(
            content.Key,
            slug,
            title,
            summary,
            level,
            focus,
            dotnetZone,
            dotnetZoneStep,
            order,
            tags,
            publishedDate,
            GetString(content, BlogContentAliases.BodyBlocks) ?? string.Empty,
            content.UpdateDate);
    }

    private CmsReorderArticleListItemDto MapReorderArticleListItem(IContent content)
    {
        var title = GetString(content, BlogContentAliases.Title)
            ?? content.Name
            ?? "Untitled";

        return new CmsReorderArticleListItemDto(
            content.Key,
            title,
            GetString(content, BlogContentAliases.Slug) ?? CreateSlug(title),
            GetString(content, BlogContentAliases.Summary) ?? string.Empty,
            GetString(content, BlogContentAliases.Level) ?? string.Empty,
            GetString(content, BlogContentAliases.Focus) ?? string.Empty,
            GetString(content, BlogContentAliases.DotnetZone) ?? DefaultDotnetZone,
            GetString(content, BlogContentAliases.DotnetZoneStep) ?? DefaultDotnetZoneStep,
            GetInt(content, BlogContentAliases.Order),
            content.UpdateDate);
    }

    private static CmsSaveArticleResponse? ValidateArticleRequest(CmsSaveArticleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return new CmsSaveArticleResponse(false, Guid.Empty, "Article title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            return new CmsSaveArticleResponse(false, Guid.Empty, "Article slug is required.");
        }

        return null;
    }

    private void ApplyArticleValues(
        IContent article,
        CmsSaveArticleRequest request,
        string slug)
    {
        var zone = string.IsNullOrWhiteSpace(request.DotnetZone)
            ? DefaultDotnetZone
            : request.DotnetZone.Trim();

        var step = string.IsNullOrWhiteSpace(request.DotnetZoneStep)
            ? DefaultDotnetZoneStep
            : request.DotnetZoneStep.Trim();

        var order = request.Order.GetValueOrDefault();

        if (order <= 0)
        {
            order = GetNextArticleOrder(zone, step, article.Key);
        }

        article.SetValue(BlogContentAliases.Title, request.Title.Trim());
        article.SetValue(BlogContentAliases.Slug, slug);
        article.SetValue(BlogContentAliases.Summary, request.Summary ?? string.Empty);
        article.SetValue(BlogContentAliases.Level, request.Level ?? string.Empty);
        article.SetValue(BlogContentAliases.Focus, request.Focus ?? string.Empty);
        article.SetValue(BlogContentAliases.DotnetZone, zone);
        article.SetValue(BlogContentAliases.DotnetZoneStep, step);
        article.SetValue(BlogContentAliases.Order, order);
        article.SetValue(BlogContentAliases.Tags, request.Tags ?? string.Empty);
        article.SetValue(BlogContentAliases.BodyBlocks, request.BodyBlocks ?? string.Empty);

        if (GetDateTimeOffset(article, BlogContentAliases.PublishedDate) is null)
        {
            article.SetValue(BlogContentAliases.PublishedDate, DateTime.UtcNow);
        }
    }

    private IReadOnlyCollection<IContent> GetRootArticles()
    {
        return _contentService
            .GetRootContent()
            .Where(content => content.ContentType.Alias == BlogContentAliases.BlogArticle)
            .ToList();
    }

    private int GetContentCount(string contentTypeAlias)
    {
        var contentType = _contentTypeService.Get(contentTypeAlias);

        if (contentType is null)
        {
            return 0;
        }

        return _contentService
            .GetPagedOfType(
                contentType.Id,
                0,
                int.MaxValue,
                out _,
                filter: null,
                ordering: null)
            .Count();
    }

    private static string? GetString(IContent content, string alias)
    {
        return content.GetValue<string>(alias);
    }

    private static int GetInt(IContent content, string alias)
    {
        return content.GetValue<int?>(alias) ?? 0;
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
