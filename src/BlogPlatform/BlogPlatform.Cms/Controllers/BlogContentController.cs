using BlogPlatform.Cms.BlogContent;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlogPlatform.Cms.Controllers;

[ApiController]
[Route("api/blog-content")]
public sealed class BlogContentController : ControllerBase
{
    private readonly IBlogContentAdminService _blogContent;
    private readonly ILogger<BlogContentController> _logger;

    public BlogContentController(IBlogContentAdminService blogContent, ILogger<BlogContentController> logger)
    {
        _blogContent = blogContent;
        _logger = logger;
    }

    [HttpGet("dotnet-roadmap")]
    public async Task<ActionResult<IReadOnlyCollection<CmsDotnetZoneListItemDto>>> GetDotnetRoadmap(
        CancellationToken cancellationToken)
    {
        return Ok(await _blogContent.GetDotnetRoadmapAsync(cancellationToken));
    }

    [HttpGet("database-summary")]
    public async Task<ActionResult<CmsDatabaseSummaryDto>> GetDatabaseSummary(
        CancellationToken cancellationToken)
    {
        return Ok(await _blogContent.GetDatabaseSummaryAsync(cancellationToken));
    }

    [HttpGet("seed-export")]
    public async Task<IActionResult> ExportSeedContent(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seed export started.");

        try
        {
            var seedContent = await _blogContent.BuildSeedContentAsync(cancellationToken);

            var zoneCount = seedContent.RoadmapZones.Count;
            var stepCount = seedContent.RoadmapZones.Sum(zone => zone.Steps.Count);
            var articleCount = seedContent.Articles.Count;
            var blockCount = seedContent.Articles.Sum(article => article.BodyBlocks.Count);

            _logger.LogInformation(
                "Seed export content built. Zones: {ZoneCount}, Steps: {StepCount}, Articles: {ArticleCount}, BodyBlocks: {BodyBlockCount}.",
                zoneCount,
                stepCount,
                articleCount,
                blockCount);

            var json = JsonSerializer.Serialize(
                seedContent,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

            _logger.LogInformation(
                "Seed export JSON serialized. Size: {JsonSizeBytes} bytes.",
                Encoding.UTF8.GetByteCount(json));

            return File(
                Encoding.UTF8.GetBytes(json),
                "application/json",
                "blog-content.seed.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seed export failed.");
            throw;
        }
    }

    [HttpGet("document-types")]
    public ActionResult<IReadOnlyCollection<CmsDocumentTypeListItemDto>> GetDocumentTypes()
    {
        return Ok(_blogContent.GetDocumentTypes());
    }

    [HttpGet("articles")]
    public ActionResult<IReadOnlyCollection<CmsArticleListItemDto>> GetArticles()
    {
        return Ok(_blogContent.GetArticles());
    }


    [HttpGet("articles/reorder")]
    public ActionResult<IReadOnlyCollection<CmsReorderArticleListItemDto>> GetArticlesForReorder(
        [FromQuery] string? zone,
        [FromQuery] string? step)
    {
        return Ok(_blogContent.GetArticlesForReorder(zone, step));
    }

    [HttpPut("articles/reorder")]
    public ActionResult<CmsReorderArticlesResponse> ReorderArticles(
        [FromBody] CmsReorderArticlesRequest request)
    {
        var result = _blogContent.ReorderArticles(request);

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    [HttpGet("articles/next-order")]
    public ActionResult<int> GetNextArticleOrder(
        [FromQuery] string? zone,
        [FromQuery] string? step,
        [FromQuery] Guid? excludeArticleKey = null)
    {
        return Ok(_blogContent.GetNextArticleOrder(
            zone,
            step,
            excludeArticleKey));
    }

    [HttpGet("articles/{key:guid}")]
    public ActionResult<CmsArticleEditorDto> GetArticle(Guid key)
    {
        var article = _blogContent.GetArticle(key);

        return article is null
            ? NotFound()
            : Ok(article);
    }

    [HttpPost("articles")]
    public async Task<ActionResult<CmsSaveArticleResponse>> CreateArticle(
        [FromBody] CmsSaveArticleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _blogContent.CreateArticleAsync(request, cancellationToken);

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    [HttpPut("articles/{key:guid}")]
    public async Task<ActionResult<CmsSaveArticleResponse>> UpdateArticle(
        Guid key,
        [FromBody] CmsSaveArticleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _blogContent.UpdateArticleAsync(key, request, cancellationToken);

        if (!result.Success && result.Message == "Article not found.")
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    [HttpDelete("articles/{key:guid}")]
    public ActionResult<CmsDeleteResponse> DeleteArticle(Guid key)
    {
        var result = _blogContent.DeleteArticle(key);

        if (!result.Success && result.Message == "Article not found.")
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result)
            : StatusCode(500, result);
    }

    [HttpDelete("document-types/{key:guid}")]
    public ActionResult<CmsDeleteResponse> DeleteDocumentType(Guid key)
    {
        var result = _blogContent.DeleteDocumentType(key);

        if (!result.Success && result.Message == "Document type not found.")
        {
            return NotFound(result);
        }

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("dotnet-roadmap/zones")]
    public async Task<ActionResult<CmsSaveRoadmapResponse>> CreateZone(
        [FromBody] CmsSaveRoadmapZoneRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _blogContent.CreateZoneAsync(request, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("dotnet-roadmap/zones/{zoneKey}")]
    public async Task<ActionResult<CmsSaveRoadmapResponse>> UpdateZone(
        string zoneKey,
        [FromBody] CmsSaveRoadmapZoneRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _blogContent.UpdateZoneAsync(zoneKey, request, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("dotnet-roadmap/zones/{zoneKey}")]
    public async Task<ActionResult<CmsDeleteResponse>> DeleteZone(
        string zoneKey,
        CancellationToken cancellationToken)
    {
        var result = await _blogContent.DeleteZoneAsync(zoneKey, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("dotnet-roadmap/zones/{zoneKey}/steps")]
    public async Task<ActionResult<CmsSaveRoadmapResponse>> CreateStep(
        string zoneKey,
        [FromBody] CmsSaveRoadmapStepRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _blogContent.CreateStepAsync(zoneKey, request, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("dotnet-roadmap/zones/{zoneKey}/steps/{stepKey}")]
    public async Task<ActionResult<CmsSaveRoadmapResponse>> UpdateStep(
        string zoneKey,
        string stepKey,
        [FromBody] CmsSaveRoadmapStepRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _blogContent.UpdateStepAsync(zoneKey, stepKey, request, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("dotnet-roadmap/zones/{zoneKey}/steps/{stepKey}")]
    public async Task<ActionResult<CmsDeleteResponse>> DeleteStep(
        string zoneKey,
        string stepKey,
        CancellationToken cancellationToken)
    {
        var result = await _blogContent.DeleteStepAsync(zoneKey, stepKey, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
