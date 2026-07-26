using LearnKit.Application.Articles.Public.Contracts;
using LearnKit.Application.Articles.Public.Models;
using LearnKit.Domain.Articles;
using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.Entities;

namespace LearnKit.Application.Articles.Public.Queries.GetArticleBySlug;

/// <summary>
/// Handles requests to retrieve articles by slug and language.
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
    /// Retrieves an article in the requested language.
    ///
    /// If a complete requested translation is unavailable,
    /// the complete default language version is returned.
    /// </summary>
    public async Task<ArticleDetails?> HandleAsync(
        GetArticleBySlugQuery query,
        CancellationToken cancellationToken = default)
    {
        var requestedLanguage =
            SupportedArticleLanguages.Normalize(query.LanguageCode);

        var article = await _articleStore.GetBySlugAsync(
            query.Slug,
            cancellationToken);

        if (article is null)
        {
            return null;
        }

        var resolvedLanguage = ResolveLanguage(
            article,
            requestedLanguage);

        if (resolvedLanguage is null)
        {
            return null;
        }

        var translation =
            article.GetTranslation(resolvedLanguage);

        if (translation is null)
        {
            return null;
        }

        var blocks = article.Blocks
            .Where(block =>
                block.GetTranslation(resolvedLanguage) is not null)
            .Select(block => MapBlock(
                block,
                resolvedLanguage))
            .ToList();

        return new ArticleDetails(
            article.Id,
            article.Slug,
            translation.Title,
            translation.Summary,
            translation.Status.ToString(),
            blocks,
            resolvedLanguage,
            !string.Equals(
                requestedLanguage,
                resolvedLanguage,
                StringComparison.Ordinal));
    }

    private static string? ResolveLanguage(
        Article article,
        string requestedLanguage)
    {
        if (article.GetTranslation(requestedLanguage) is not null)
        {
            return requestedLanguage;
        }

        if (article.GetTranslation(SupportedArticleLanguages.Default) is not null)
        {
            return SupportedArticleLanguages.Default;
        }

        return null;
    }

    private static ArticleBlockDetails MapBlock(
        ArticleBlock block,
        string languageCode)
    {
        var translation = block.GetTranslation(languageCode)
            ?? throw new InvalidOperationException(
                $"Block '{block.Id}' does not contain " +
                $"the resolved language '{languageCode}'.");

        return new ArticleBlockDetails(
            block.Id,
            block.Type.ToString(),
            block.SortOrder,
            translation.ContentJson);
    }
}
