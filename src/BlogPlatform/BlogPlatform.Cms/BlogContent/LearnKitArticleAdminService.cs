using System.Text.Json;
using LearnKit.Application.Articles.Admin.Commands.CreateArticle;
using LearnKit.Application.Articles.Admin.Commands.CreateArticleBlock;
using LearnKit.Application.Articles.Admin.Commands.DeleteArticle;
using LearnKit.Application.Articles.Admin.Commands.DeleteArticleBlock;
using LearnKit.Application.Articles.Admin.Commands.ReorderArticleBlocks;
using LearnKit.Application.Articles.Admin.Commands.ReorderArticles;
using LearnKit.Application.Articles.Admin.Commands.PublishArticle;
using LearnKit.Application.Articles.Admin.Commands.UpdateArticle;
using LearnKit.Application.Articles.Admin.Commands.UpdateArticleBlock;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Application.Articles.Admin.Queries.GetArticleForEditing;
using LearnKit.Application.Articles.Admin.Queries.GetArticlesForManagement;
using LearnKit.Application.Roadmaps.Admin.Models;
using LearnKit.Application.Roadmaps.Admin.Queries.GetLearningPathForManagement;

namespace BlogPlatform.Cms.BlogContent;

public sealed class LearnKitArticleAdminService : ILearnKitArticleAdminService
{
    private const string LearningPathKey = "dotnet";
    private readonly GetArticlesForManagementHandler _getArticles;
    private readonly GetArticleForEditingHandler _getArticle;
    private readonly GetLearningPathForManagementHandler _getPath;
    private readonly CreateArticleHandler _createArticle;
    private readonly UpdateArticleHandler _updateArticle;
    private readonly DeleteArticleHandler _deleteArticle;
    private readonly ReorderArticlesHandler _reorderArticles;
    private readonly PublishArticleHandler _publishArticle;
    private readonly CreateArticleBlockHandler _createBlock;
    private readonly UpdateArticleBlockHandler _updateBlock;
    private readonly DeleteArticleBlockHandler _deleteBlock;
    private readonly ReorderArticleBlocksHandler _reorderBlocks;

    public LearnKitArticleAdminService(
        GetArticlesForManagementHandler getArticles,
        GetArticleForEditingHandler getArticle,
        GetLearningPathForManagementHandler getPath,
        CreateArticleHandler createArticle,
        UpdateArticleHandler updateArticle,
        DeleteArticleHandler deleteArticle,
        ReorderArticlesHandler reorderArticles,
        PublishArticleHandler publishArticle,
        CreateArticleBlockHandler createBlock,
        UpdateArticleBlockHandler updateBlock,
        DeleteArticleBlockHandler deleteBlock,
        ReorderArticleBlocksHandler reorderBlocks)
    {
        _getArticles = getArticles; _getArticle = getArticle; _getPath = getPath;
        _createArticle = createArticle; _updateArticle = updateArticle; _deleteArticle = deleteArticle;
        _reorderArticles = reorderArticles; _publishArticle = publishArticle; _createBlock = createBlock; _updateBlock = updateBlock;
        _deleteBlock = deleteBlock; _reorderBlocks = reorderBlocks;
    }

    public async Task<IReadOnlyCollection<CmsArticleListItemDto>> GetArticlesAsync(CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(cancellationToken);
        var articles = await _getArticles.HandleAsync(new(), cancellationToken);
        return articles.Select(article => MapListItem(article, context)).ToList();
    }

    public async Task<CmsArticleEditorDto?> GetArticleAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(cancellationToken);
        var article = await _getArticle.HandleAsync(new(key), cancellationToken);
        return article is null ? null : MapEditor(article, context);
    }

    public async Task<int> GetNextArticleOrderAsync(string? zone, string? step, Guid? excludeArticleKey, CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(cancellationToken);
        var stepId = ResolveStep(context, zone, step).Id;
        var articles = await _getArticles.HandleAsync(new(), cancellationToken);
        return articles.Where(a => a.LearningStepId == stepId && a.Id != excludeArticleKey)
            .Select(a => a.SortOrder).DefaultIfEmpty(0).Max() + 1;
    }

    public async Task<IReadOnlyCollection<CmsReorderArticleListItemDto>> GetArticlesForReorderAsync(string? zone, string? step, CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(cancellationToken);
        var stepDetails = ResolveStep(context, zone, step);
        var articles = await _getArticles.HandleAsync(new(), cancellationToken);
        return articles.Where(a => a.LearningStepId == stepDetails.Id).OrderBy(a => a.SortOrder)
            .Select(a => new CmsReorderArticleListItemDto(a.Id,a.Title,a.Slug,a.Summary,a.Status,"LearnKit",zone ?? "",step ?? "",a.SortOrder,DateTime.UnixEpoch)).ToList();
    }

    public async Task<CmsReorderArticlesResponse> ReorderArticlesAsync(CmsReorderArticlesRequest request, CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(cancellationToken);
        var step = ResolveStep(context, request.DotnetZone, request.DotnetZoneStep);
        var success = await _reorderArticles.HandleAsync(new(step.Id, request.ArticleKeys), cancellationToken);
        return new(success, success ? "Article order saved." : "Article order is invalid.",
            success ? await GetArticlesForReorderAsync(request.DotnetZone, request.DotnetZoneStep, cancellationToken) : []);
    }

    public async Task<CmsSaveArticleResponse> CreateArticleAsync(CmsSaveArticleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var context = await GetContextAsync(cancellationToken);
            var step = ResolveStep(context, request.DotnetZone, request.DotnetZoneStep);
            var id = await _createArticle.HandleAsync(new(step.Id, request.Slug, request.Title, request.Summary, request.Order ?? 1), cancellationToken);
            await SynchronizeBlocksAsync(id, request.BodyBlocks, cancellationToken);
            await _publishArticle.HandleAsync(new(id), cancellationToken);
            return new(true, id, "Article created and published in LearnKit.");
        }
        catch (Exception ex) when (ex is ArgumentException or JsonException)
        {
            return new(false, Guid.Empty, ex.Message);
        }
    }

    public async Task<CmsSaveArticleResponse> UpdateArticleAsync(Guid key, CmsSaveArticleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var context = await GetContextAsync(cancellationToken);
            var step = ResolveStep(context, request.DotnetZone, request.DotnetZoneStep);
            var success = await _updateArticle.HandleAsync(new(key, step.Id, request.Slug, request.Title, request.Summary, request.Order ?? 1), cancellationToken);
            if (!success) return new(false, key, "Article not found.");
            await SynchronizeBlocksAsync(key, request.BodyBlocks, cancellationToken);
            await _publishArticle.HandleAsync(new(key), cancellationToken);
            return new(true, key, "Article updated and published in LearnKit.");
        }
        catch (Exception ex) when (ex is ArgumentException or JsonException)
        {
            return new(false, key, ex.Message);
        }
    }

    public async Task<CmsDeleteResponse> DeleteArticleAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var success = await _deleteArticle.HandleAsync(new(key), cancellationToken);
        return new(success, success ? "Article deleted from LearnKit." : "Article not found.");
    }

    private async Task SynchronizeBlocksAsync(Guid articleId, string bodyBlocks, CancellationToken cancellationToken)
    {
        var article = await _getArticle.HandleAsync(new(articleId), cancellationToken) ?? throw new ArgumentException("Article not found.");
        var requested = ParseBlocks(bodyBlocks);
        var retained = new HashSet<Guid>();
        var ordered = new List<Guid>();

        for (var index = 0; index < requested.Count; index++)
        {
            var item = requested[index];
            Guid blockId;
            if (item.BlockId.HasValue && article.Blocks.Any(block => block.Id == item.BlockId.Value))
            {
                blockId = item.BlockId.Value;
                await _updateBlock.HandleAsync(new(articleId, blockId, item.Type, item.ContentJson), cancellationToken);
            }
            else
            {
                blockId = (await _createBlock.HandleAsync(new(articleId, item.Type, index + 1, item.ContentJson), cancellationToken))!.Value;
            }
            retained.Add(blockId); ordered.Add(blockId);
        }

        foreach (var existing in article.Blocks.Where(block => !retained.Contains(block.Id)))
            await _deleteBlock.HandleAsync(new(articleId, existing.Id), cancellationToken);

        if (ordered.Count > 0)
            await _reorderBlocks.HandleAsync(new(articleId, ordered), cancellationToken);
    }

    private static List<RequestedBlock> ParseBlocks(string bodyBlocks)
    {
        using var document = JsonDocument.Parse(bodyBlocks);
        if (!document.RootElement.TryGetProperty("contentData", out var contentData) || contentData.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Article body must use the block-list JSON format.");
        var result = new List<RequestedBlock>();
        foreach (var element in contentData.EnumerateArray())
        {
            Guid? id = null;
            if (element.TryGetProperty("learnKitBlockId", out var idElement) && Guid.TryParse(idElement.GetString(), out var parsed)) id = parsed;
            result.Add(new RequestedBlock(id, ResolveBlockType(element), element.GetRawText()));
        }
        return result;
    }

    private static string ResolveBlockType(JsonElement element)
    {
        var sourceType = element.TryGetProperty("sourceType", out var source) ? source.GetString()?.ToLowerInvariant() : null;
        return sourceType switch
        {
            "plantumldiagram" or "mermaiddiagram" => "Diagram",
            "codesnippet" => "Code",
            "table" => "Table",
            "callout" => "Callout",
            _ => "Markdown"
        };
    }

    private static CmsArticleEditorDto MapEditor(ArticleManagementDetails article, AdminContext context)
    {
        var step = context.StepsById[article.LearningStepId];
        return new(article.Id,article.Title,article.Slug,article.Summary,article.Status,"LearnKit",step.ZoneKey,step.Step.Key,article.SortOrder,"learnkit",BuildBodyBlocks(article),true);
    }

    private static CmsArticleListItemDto MapListItem(ArticleManagementListItem article, AdminContext context)
    {
        var step = context.StepsById[article.LearningStepId];
        return new(article.Id,article.Slug,article.Title,article.Summary,article.Status,"LearnKit",step.ZoneKey,step.Step.Key,article.SortOrder,[],null,string.Empty,DateTime.UnixEpoch);
    }

    private static string BuildBodyBlocks(ArticleManagementDetails article)
    {
        var contentData = article.Blocks.OrderBy(block => block.SortOrder).Select(block =>
        {
            using var document = JsonDocument.Parse(block.ContentJson);
            var values = new Dictionary<string, object?>();
            foreach (var property in document.RootElement.EnumerateObject()) values[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
            values["learnKitBlockId"] = block.Id;
            values["udi"] = $"umb://element/{block.Id:N}";
            return values;
        }).ToList();
        var layout = contentData.Select(item => new { contentUdi = item["udi"] }).ToList();
        return JsonSerializer.Serialize(new { layout = new Dictionary<string, object> { ["Umbraco.BlockList"] = layout }, contentData, settingsData = Array.Empty<object>() });
    }

    private async Task<AdminContext> GetContextAsync(CancellationToken cancellationToken)
    {
        var path = await _getPath.HandleAsync(new(LearningPathKey), cancellationToken) ?? throw new InvalidOperationException("LearnKit learning path was not found.");
        var steps = path.Zones.SelectMany(zone => zone.Steps.Select(step => new StepContext(zone.Key, step))).ToList();
        return new(path, steps.ToDictionary(item => item.Step.Id));
    }

    private static LearningStepManagementDetails ResolveStep(AdminContext context, string? zoneKey, string? stepKey)
    {
        return context.Path.Zones.FirstOrDefault(zone => zone.Key == zoneKey)?.Steps.FirstOrDefault(step => step.Key == stepKey)
            ?? throw new ArgumentException("Selected learning step was not found in LearnKit.");
    }

    private sealed record RequestedBlock(Guid? BlockId, string Type, string ContentJson);
    private sealed record StepContext(string ZoneKey, LearningStepManagementDetails Step);
    private sealed record AdminContext(LearningPathManagementDetails Path, IReadOnlyDictionary<Guid, StepContext> StepsById);
}
