using LearnKit.Application.Articles.Admin.Commands.CreateArticle;
using LearnKit.Application.Articles.Admin.Commands.CreateArticleBlock;
using LearnKit.Application.Articles.Admin.Commands.DeleteArticle;
using LearnKit.Application.Articles.Admin.Commands.DeleteArticleBlock;
using LearnKit.Application.Articles.Admin.Commands.ReorderArticleBlocks;
using LearnKit.Application.Articles.Admin.Commands.ReorderArticles;
using LearnKit.Application.Articles.Admin.Commands.UpdateArticleBlock;
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
    private readonly CreateArticleHandler _createArticleHandler;
    private readonly CreateArticleBlockHandler _createArticleBlockHandler;
    private readonly UpdateArticleBlockHandler _updateArticleBlockHandler;
    private readonly DeleteArticleBlockHandler _deleteArticleBlockHandler;
    private readonly DeleteArticleHandler _deleteArticleHandler;
    private readonly ReorderArticleBlocksHandler _reorderArticleBlocksHandler;
    private readonly ReorderArticlesHandler _reorderArticlesHandler;
    private readonly PublishArticleHandler _publishArticleHandler;
    private readonly UnpublishArticleHandler _unpublishArticleHandler;
    private readonly UpdateArticleHandler _updateArticleHandler;

    /// <summary>
    /// Creates a new article management controller.
    /// </summary>
    public ArticlesManagementController(
        GetArticlesForManagementHandler getArticlesForManagementHandler,
        GetArticleForEditingHandler getArticleForEditingHandler,
        CreateArticleHandler createArticleHandler,
        CreateArticleBlockHandler createArticleBlockHandler,
        UpdateArticleBlockHandler updateArticleBlockHandler,
        DeleteArticleBlockHandler deleteArticleBlockHandler,
        DeleteArticleHandler deleteArticleHandler,
        ReorderArticleBlocksHandler reorderArticleBlocksHandler,
        ReorderArticlesHandler reorderArticlesHandler,
        PublishArticleHandler publishArticleHandler,
        UnpublishArticleHandler unpublishArticleHandler,
        UpdateArticleHandler updateArticleHandler)
    {
        _getArticlesForManagementHandler = getArticlesForManagementHandler;
        _getArticleForEditingHandler = getArticleForEditingHandler;
        _createArticleHandler = createArticleHandler;
        _createArticleBlockHandler = createArticleBlockHandler;
        _updateArticleBlockHandler = updateArticleBlockHandler;
        _deleteArticleBlockHandler = deleteArticleBlockHandler;
        _deleteArticleHandler = deleteArticleHandler;
        _reorderArticleBlocksHandler = reorderArticleBlocksHandler;
        _reorderArticlesHandler = reorderArticlesHandler;
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
    /// Creates a new draft article.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateArticleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateArticleCommand(
            request.LearningStepId,
            request.Slug,
            request.Title,
            request.Summary,
            request.SortOrder);

        var articleId = await _createArticleHandler.HandleAsync(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { articleId },
            new { articleId });
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
            request.LearningStepId,
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
    /// Deletes an article.
    /// </summary>
    [HttpDelete("{articleId:guid}")]
    public async Task<IActionResult> Delete(
        Guid articleId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteArticleCommand(articleId);

        var deleted = await _deleteArticleHandler.HandleAsync(
            command,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Reorders all articles assigned to one learning step.
    /// </summary>
    [HttpPut("order")]
    public async Task<IActionResult> Reorder(
        [FromBody] ReorderArticlesRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderArticlesCommand(
            request.LearningStepId,
            request.OrderedArticleIds);

        var reordered = await _reorderArticlesHandler.HandleAsync(
            command,
            cancellationToken);

        if (!reordered)
        {
            return BadRequest();
        }

        return NoContent();
    }

    /// <summary>
    /// Adds a block to an article.
    /// </summary>
    [HttpPost("{articleId:guid}/blocks")]
    public async Task<IActionResult> CreateBlock(
        Guid articleId,
        [FromBody] CreateArticleBlockRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateArticleBlockCommand(
            articleId,
            request.Type,
            request.SortOrder,
            request.ContentJson);

        var blockId = await _createArticleBlockHandler.HandleAsync(
            command,
            cancellationToken);

        if (blockId is null)
        {
            return NotFound();
        }

        return CreatedAtAction(
            nameof(GetById),
            new { articleId },
            new { blockId });
    }

    /// <summary>
    /// Updates an article block.
    /// </summary>
    [HttpPut("{articleId:guid}/blocks/{blockId:guid}")]
    public async Task<IActionResult> UpdateBlock(
        Guid articleId,
        Guid blockId,
        [FromBody] UpdateArticleBlockRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateArticleBlockCommand(
            articleId,
            blockId,
            request.Type,
            request.ContentJson);

        var updated = await _updateArticleBlockHandler.HandleAsync(
            command,
            cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes an article block.
    /// </summary>
    [HttpDelete("{articleId:guid}/blocks/{blockId:guid}")]
    public async Task<IActionResult> DeleteBlock(
        Guid articleId,
        Guid blockId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteArticleBlockCommand(articleId, blockId);

        var deleted = await _deleteArticleBlockHandler.HandleAsync(
            command,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Reorders all blocks in an article.
    /// </summary>
    [HttpPut("{articleId:guid}/blocks/order")]
    public async Task<IActionResult> ReorderBlocks(
        Guid articleId,
        [FromBody] ReorderArticleBlocksRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderArticleBlocksCommand(
            articleId,
            request.OrderedBlockIds);

        var reordered = await _reorderArticleBlocksHandler.HandleAsync(
            command,
            cancellationToken);

        if (!reordered)
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
