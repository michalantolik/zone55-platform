using LearnKit.Application.Articles.Admin.Commands.PublishArticle;
using BlogPlatform.Api.Controllers.LearnKit.Admin.Models;
using LearnKit.Application.Articles.Admin.Commands.UnpublishArticle;
using LearnKit.Application.Articles.Admin.Commands.UpdateArticle;
using LearnKit.Application.Articles.Admin.Queries.GetArticleForEditing;
using LearnKit.Application.Articles.Admin.Queries.GetArticlesForManagement;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers.LearnKit.Admin;

/// <summary>
/// Exposes LearnKit article management endpoints.
/// </summary>
[ApiController]
[Route("api/learnkit/admin/articles")]
public sealed class ArticlesManagementController : ControllerBase
{
    private readonly GetArticlesForManagementHandler _getArticlesForManagementHandler;
    private readonly GetArticleForEditingHandler _getArticleForEditingHandler;
    private readonly PublishArticleHandler _publishArticleHandler;
    private readonly UnpublishArticleHandler _unpublishArticleHandler;
    private readonly UpdateArticleHandler _updateArticleHandler;

    /// <summary>
    /// Creates a new article management controller.
    /// </summary>
    public ArticlesManagementController(
        GetArticlesForManagementHandler getArticlesForManagementHandler,
        GetArticleForEditingHandler getArticleForEditingHandler,
        PublishArticleHandler publishArticleHandler,
        UnpublishArticleHandler unpublishArticleHandler,
        UpdateArticleHandler updateArticleHandler)
    {
        _getArticlesForManagementHandler = getArticlesForManagementHandler;
        _getArticleForEditingHandler = getArticleForEditingHandler;
        _publishArticleHandler = publishArticleHandler;
        _unpublishArticleHandler = unpublishArticleHandler;
        _updateArticleHandler = updateArticleHandler;
    }

    /// <summary>
    /// Returns all articles available for management.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var query = new GetArticlesForManagementQuery();

        var articles = await _getArticlesForManagementHandler.HandleAsync(
            query,
            cancellationToken);

        return Ok(articles);
    }

    /// <summary>
    /// Returns an article for editing.
    /// </summary>
    [HttpGet("{articleId:guid}")]
    public async Task<IActionResult> GetById(
        Guid articleId,
        CancellationToken cancellationToken)
    {
        var query = new GetArticleForEditingQuery(articleId);

        var article = await _getArticleForEditingHandler.HandleAsync(
            query,
            cancellationToken);

        if (article is null)
        {
            return NotFound();
        }

        return Ok(article);
    }

    /// <summary>
    /// Updates basic article details.
    /// </summary>
    [HttpPut("{articleId:guid}")]
    public async Task<IActionResult> Update(
        Guid articleId,
        [FromBody] UpdateArticleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateArticleCommand(
            articleId,
            request.Slug,
            request.Title,
            request.Summary,
            request.SortOrder);

        var updated = await _updateArticleHandler.HandleAsync(
            command,
            cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Publishes an article.
    /// </summary>
    [HttpPost("{articleId:guid}/publish")]
    public async Task<IActionResult> Publish(
        Guid articleId,
        CancellationToken cancellationToken)
    {
        var command = new PublishArticleCommand(articleId);

        var published = await _publishArticleHandler.HandleAsync(
            command,
            cancellationToken);

        if (!published)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Moves a published article back to draft.
    /// </summary>
    [HttpPost("{articleId:guid}/unpublish")]
    public async Task<IActionResult> Unpublish(
        Guid articleId,
        CancellationToken cancellationToken)
    {
        var command = new UnpublishArticleCommand(articleId);

        var unpublished = await _unpublishArticleHandler.HandleAsync(
            command,
            cancellationToken);

        if (!unpublished)
        {
            return NotFound();
        }

        return NoContent();
    }

}
