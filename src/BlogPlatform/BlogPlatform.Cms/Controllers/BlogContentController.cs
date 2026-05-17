using BlogPlatform.Cms.BlogContent;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Cms.Controllers;

[ApiController]
[Route("api/blog-content")]
public sealed class BlogContentController : ControllerBase
{
    private readonly IBlogContentAdminService _blogContent;

    public BlogContentController(IBlogContentAdminService blogContent)
    {
        _blogContent = blogContent;
    }

    [HttpGet("dotnet-roadmap")]
    public async Task<ActionResult<IReadOnlyCollection<CmsDotnetZoneListItemDto>>> GetDotnetRoadmap(
        CancellationToken cancellationToken)
    {
        return Ok(await _blogContent.GetDotnetRoadmapAsync(cancellationToken));
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
