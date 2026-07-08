using LearnKit.Application.Articles.Contracts;
using LearnKit.Application.Articles.Models;

namespace LearnKit.Application.Articles.Queries.GetArticleBySlug;

/// <summary>
/// Handles requests to retrieve articles by slug.
/// </summary>
public sealed class GetArticleBySlugHandler
{
    private readonly IArticleStore _articleStore;

    /// <summary>
    /// Creates a new handler.
    /// </summary>
    public GetArticleBySlugHandler(
        IArticleStore articleStore)
    {
        _articleStore = articleStore;
    }

    /// <summary>
    /// Retrieves an article by its slug.
    /// </summary>
    public async Task<ArticleDetails?> HandleAsync(
        GetArticleBySlugQuery query,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleStore.GetBySlugAsync(
            query.Slug,
            cancellationToken);

        if (article is null)
        {
            return null;
        }

        return new ArticleDetails(
            article.Id,
            article.Slug,
            article.Title,
            article.Summary,
            article.Status.ToString(),
            article.Blocks
                .Select(block => new ArticleBlockDetails(
                    block.Id,
                    block.Type.ToString(),
                    block.SortOrder,
                    block.ContentJson))
                .ToList());
    }
}
