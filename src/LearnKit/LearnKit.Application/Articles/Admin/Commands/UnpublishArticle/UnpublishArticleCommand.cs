namespace LearnKit.Application.Articles.Admin.Commands.UnpublishArticle;

/// <summary>
/// Requests moving an article back to draft.
/// </summary>
public sealed record UnpublishArticleCommand(Guid ArticleId);
