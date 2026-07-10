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

    /// <summary>
    /// Creates a new article management controller.
    /// </summary>
    public ArticlesManagementController(
        GetArticlesForManagementHandler getArticlesForManagementHandler,
        GetArticleForEditingHandler getArticleForEditingHandler)
    {
        _getArticlesForManagementHandler = getArticlesForManagementHandler;
        _getArticleForEditingHandler = getArticleForEditingHandler;
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
}
