namespace LearnKit.Application.Articles.Queries.GetArticleBySlug;

/// <summary>
/// Represents a request to retrieve an article by its slug.
/// </summary>
/// <param name="Slug">
/// Unique article slug.
/// </param>
public sealed record GetArticleBySlugQuery(
    string Slug);
