using LearnKit.Application.Articles.Public.Queries.GetArticleBySlug;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers.LearnKit.Public
{
    /// <summary>
    /// Exposes LearnKit article endpoints.
    /// </summary>
    [ApiController]
    [Route("api/learnkit/articles")]
    public class ArticlesController : ControllerBase
    {
        private readonly GetArticleBySlugHandler _getArticleBySlugHandler;

        public ArticlesController(GetArticleBySlugHandler getArticleBySlugHandler)
        {
            _getArticleBySlugHandler = getArticleBySlugHandler;
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            var query = new GetArticleBySlugQuery(slug);
            var article = await _getArticleBySlugHandler.HandleAsync(query, cancellationToken);

            if (article is null)
            {
                return NotFound();
            }

            return Ok(article);
        }
    }
}
