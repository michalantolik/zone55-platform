using BlogPlatform.Cms.BlogContent;
using BlogPlatform.Cms.Security;
using BlogPlatform.Cms.Seeding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlogPlatform.Cms.Controllers;

[ApiController]
[Route("api/blog-content")]
[UmbracoBackOfficeOnly]
public sealed class BlogContentController : ControllerBase
{
    private readonly IBlogContentAdminService _blogContent;
    private readonly ILearnKitArticleAdminService _learnKitArticles;
    private readonly ILogger<BlogContentController> _logger;
    private readonly BlogContentSeedOperationsOptions _seedOptions;
    private readonly BlogSeedImportJobService _seedImportJobs;

    public BlogContentController(
        IBlogContentAdminService blogContent,
        ILearnKitArticleAdminService learnKitArticles,
        ILogger<BlogContentController> logger,
        IOptions<BlogContentSeedOperationsOptions> seedOptions,
        BlogSeedImportJobService seedImportJobs)
    {
        _blogContent = blogContent;
        _learnKitArticles = learnKitArticles;
        _logger = logger;
        _seedOptions = seedOptions.Value;
        _seedImportJobs = seedImportJobs;
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

        var seedContent = await _blogContent.BuildSeedContentAsync(cancellationToken);

        var json = JsonSerializer.Serialize(
            seedContent,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

        return File(
            Encoding.UTF8.GetBytes(json),
            "application/json",
            "blog-content.seed.json");
    }

    [HttpPost("seed-import/admin")]
    public async Task<ActionResult<CmsSeedImportResponse>> ImportSeedContentFromAdmin(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin seed import requested.");

        var result = await _blogContent.ImportSeedContentAsync(cancellationToken);

        return Ok(result);
    }


    [AllowAnonymous]
    [HttpPost("seed-import/github/start")]
    public ActionResult<BlogSeedImportJobSnapshot> StartSeedContentImportFromGitHub()
    {
        var unauthorized = ValidateGitHubSeedApiKey();

        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var started = _seedImportJobs.TryStart(out var snapshot);

        return started
            ? Accepted(snapshot)
            : Conflict(snapshot);
    }

    [AllowAnonymous]
    [HttpGet("seed-import/github/status")]
    public ActionResult<BlogSeedImportJobSnapshot> GetSeedContentImportStatus()
    {
        var unauthorized = ValidateGitHubSeedApiKey();

        if (unauthorized is not null)
        {
            return unauthorized;
        }

        return Ok(_seedImportJobs.GetSnapshot());
    }

    [AllowAnonymous]
    [HttpPost("seed-import/github")]
    public async Task<ActionResult<CmsSeedImportResponse>> ImportSeedContentFromGitHub(
        CancellationToken cancellationToken)
    {
        var unauthorized = ValidateGitHubSeedApiKey();

        if (unauthorized is not null)
        {
            return unauthorized;
        }

        _logger.LogInformation("GitHub seed import requested.");

        var result = await _blogContent.ImportSeedContentAsync(cancellationToken);

        return Ok(result);
    }

    private ActionResult? ValidateGitHubSeedApiKey()
    {
        if (string.IsNullOrWhiteSpace(_seedOptions.ApiKey))
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new CmsSeedImportResponse(
                    false,
                    "Seed API key is not configured.",
                    0,
                    0,
                    0,
                    DateTimeOffset.UtcNow));
        }

        if (!Request.Headers.TryGetValue("X-BlogPlatform-Seed-Key", out var providedKey) ||
            providedKey != _seedOptions.ApiKey)
        {
            return Unauthorized(new CmsSeedImportResponse(
                false,
                "Invalid seed API key.",
                0,
                0,
                0,
                DateTimeOffset.UtcNow));
        }

        return null;
    }

    [HttpGet("document-types")]
    public ActionResult<IReadOnlyCollection<CmsDocumentTypeListItemDto>> GetDocumentTypes()
    {
        return Ok(_blogContent.GetDocumentTypes());
    }

    [AllowAnonymous]
    [HttpGet("articles")]
    public async Task<ActionResult<IReadOnlyCollection<CmsArticleListItemDto>>> GetArticles(CancellationToken cancellationToken)
    {
        return Ok(await _learnKitArticles.GetArticlesAsync(cancellationToken));
    }

    [HttpGet("articles/reorder")]
    public async Task<ActionResult<IReadOnlyCollection<CmsReorderArticleListItemDto>>> GetArticlesForReorder(
        [FromQuery] string? zone,
        [FromQuery] string? step,
        CancellationToken cancellationToken)
    {
        return Ok(await _learnKitArticles.GetArticlesForReorderAsync(zone, step, cancellationToken));
    }

    [HttpPut("articles/reorder")]
    public async Task<ActionResult<CmsReorderArticlesResponse>> ReorderArticles(
        [FromBody] CmsReorderArticlesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _learnKitArticles.ReorderArticlesAsync(request, cancellationToken);

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    [HttpGet("articles/next-order")]
    public async Task<ActionResult<int>> GetNextArticleOrder(
        [FromQuery] string? zone,
        [FromQuery] string? step,
        [FromQuery] Guid? excludeArticleKey,
        CancellationToken cancellationToken)
    {
        return Ok(await _learnKitArticles.GetNextArticleOrderAsync(zone, step, excludeArticleKey, cancellationToken));
    }

    [HttpGet("articles/{key:guid}")]
    public async Task<ActionResult<CmsArticleEditorDto>> GetArticle(Guid key, CancellationToken cancellationToken)
    {
        var article = await _learnKitArticles.GetArticleAsync(key, cancellationToken);

        return article is null
            ? NotFound()
            : Ok(article);
    }

    [HttpPost("articles")]
    public async Task<ActionResult<CmsSaveArticleResponse>> CreateArticle(
        [FromBody] CmsSaveArticleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _learnKitArticles.CreateArticleAsync(request, cancellationToken);

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
        var result = await _learnKitArticles.UpdateArticleAsync(key, request, cancellationToken);

        if (!result.Success && result.Message == "Article not found.")
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    [HttpDelete("articles/{key:guid}")]
    public async Task<ActionResult<CmsDeleteResponse>> DeleteArticle(Guid key, CancellationToken cancellationToken)
    {
        var result = await _learnKitArticles.DeleteArticleAsync(key, cancellationToken);

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

        return result.Success
            ? Ok(result)
            : BadRequest(result);
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
