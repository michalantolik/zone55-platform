using LearnKit.Application.Articles.Public.Queries.GetArticleBySlug;
using LearnKit.Domain.Articles.BusinessRules;
using Microsoft.AspNetCore.Mvc;

namespace Backend55.Api.Controllers.LearnKit.Public;

/// <summary>
/// Exposes public LearnKit article endpoints.
/// </summary>
[ApiController]
[Route("api/learnkit/articles")]
public sealed class ArticlesController : ControllerBase
{
    private readonly GetArticleBySlugHandler _getArticleBySlugHandler;

    public ArticlesController(
        GetArticleBySlugHandler getArticleBySlugHandler)
    {
        _getArticleBySlugHandler = getArticleBySlugHandler;
    }

    /// <summary>
    /// Returns an article in the requested language.
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(
        string slug,
        [FromQuery] string language = SupportedArticleLanguages.Default,
        CancellationToken cancellationToken = default)
    {
        if (!SupportedArticleLanguages.IsSupported(language))
        {
            return BadRequest(
                new
                {
                    Error = $"Unsupported content language '{language}'.",
                    SupportedLanguages = SupportedArticleLanguages.All
                });
        }

        var query = new GetArticleBySlugQuery(
            slug,
            language);

        var article = await _getArticleBySlugHandler.HandleAsync(
            query,
            cancellationToken);

        if (article is null)
        {
            return NotFound();
        }

        return Ok(article);
    }
}
