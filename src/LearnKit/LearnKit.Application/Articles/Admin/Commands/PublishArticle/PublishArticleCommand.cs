namespace LearnKit.Application.Articles.Admin.Commands.PublishArticle;

/// <summary>
/// Represents a request to publish an article.
/// </summary>
/// <param name="ArticleId">
/// Unique article identifier.
/// </param>
public sealed record PublishArticleCommand(
    Guid ArticleId);
