using BlogPlatform.Application.Roadmap;
using BlogPlatform.Cms.Seeding;
using BlogPlatform.Cms.Seeding.Blocks;
using System.Text;
using System.Text.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace BlogPlatform.Cms.BlogContent;

public sealed class BlogContentAdminService : IBlogContentAdminService
{
    private const string DefaultDotnetZone = "foundation";
    private const string DefaultDotnetZoneStep = "basic-syntax";

    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<BlogContentAdminService> _logger;
    private readonly IRoadmapQueryService _roadmapQueries;
    private readonly IRoadmapCommandService _roadmapCommands;
    private readonly BlogSeedBlockSerializationService _blockSerialization;
    private readonly BlogContentSeeder _contentSeeder;

    public BlogContentAdminService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        ILogger<BlogContentAdminService> logger,
        IRoadmapQueryService roadmapQueries,
        IRoadmapCommandService roadmapCommands,
        BlogSeedBlockSerializationService blockSerialization,
        BlogContentSeeder contentSeeder)
    {
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _logger = logger;
        _roadmapQueries = roadmapQueries;
        _roadmapCommands = roadmapCommands;
        _blockSerialization = blockSerialization;
        _contentSeeder = contentSeeder;
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


        return new CmsSaveRoadmapResponse(result.Success, result.Message);
    }

    public async Task<CmsDeleteResponse> DeleteZoneAsync(
        string zoneKey,
        CancellationToken cancellationToken)
    {
        var result = await _roadmapCommands.DeleteZoneAsync(
            zoneKey,
            cancellationToken);


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


        return new CmsDeleteResponse(result.Success, result.Message);
    }

    public async Task<CmsSeedImportResponse> ImportSeedContentAsync(
    CancellationToken cancellationToken)
    {
        _logger.LogInformation("Manual blog seed import started.");

        await _contentSeeder.SeedAsync();


        var summary = await GetDatabaseSummaryAsync(cancellationToken);

        _logger.LogInformation(
            "Manual blog seed import completed. Zones: {Zones}, Steps: {Steps}, Articles: {Articles}.",
            summary.Zones,
            summary.ZoneSteps,
            summary.Articles);

        return new CmsSeedImportResponse(
            true,
            "Seed import completed successfully.",
            summary.Zones,
            summary.ZoneSteps,
            summary.Articles,
            DateTimeOffset.UtcNow);
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
                _logger.LogWarning(
                    "Seed export failed because Block List JSON has no contentData array. Article: {ArticleName}.",
                    content.Name);

                return [];
            }

            var blocks = new List<BlogSeedBlock>();

            foreach (var blockElement in contentData.EnumerateArray())
            {
                var block =
                    _blockSerialization.ExportFromBlockListContent(blockElement);

                _logger.LogInformation(
                    "Seed export block processed. Article: {ArticleName}, BlockAlias: {BlockAlias}, Exported: {Exported}.",
                    content.Name,
                    _blockSerialization.ResolveElementTypeAlias(blockElement) ?? "unknown",
                    block is not null);

                if (block is not null)
                {
                    blocks.Add(block);
                }
            }

            _logger.LogInformation(
                "Seed export completed. Article: {ArticleName}, ExportedBlocks: {BlockCount}.",
                content.Name,
                blocks.Count);

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

}
