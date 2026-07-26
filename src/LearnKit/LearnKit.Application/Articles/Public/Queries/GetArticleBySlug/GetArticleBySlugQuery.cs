using LearnKit.Domain.Articles.BusinessRules;

namespace LearnKit.Application.Articles.Public.Queries.GetArticleBySlug;

/// <summary>
/// Represents a request to retrieve an article
/// by its slug and content language.
/// </summary>
/// <param name="Slug">
/// Unique article slug shared by every language.
/// </param>
/// <param name="LanguageCode">
/// Requested article language.
/// </param>
public sealed record GetArticleBySlugQuery(
    string Slug,
    string LanguageCode = SupportedArticleLanguages.Default);
