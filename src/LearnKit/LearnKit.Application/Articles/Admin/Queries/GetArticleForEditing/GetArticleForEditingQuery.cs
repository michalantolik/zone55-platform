namespace LearnKit.Application.Articles.Admin.Queries.GetArticleForEditing;

/// <summary>
/// Represents a request to retrieve an article for editing.
/// </summary>
/// <param name="ArticleId">
/// Unique article identifier.
/// </param>
public sealed record GetArticleForEditingQuery(
    Guid ArticleId);
