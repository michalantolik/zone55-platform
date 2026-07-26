using LearnKit.Domain.Articles.BusinessRules;

namespace LearnKit.Application.Articles.Admin.Commands.UnpublishArticle;

/// <summary>
/// Requests moving an article back to draft.
/// </summary>
public sealed record UnpublishArticleCommand(
    Guid ArticleId,
    string LanguageCode = SupportedArticleLanguages.Default);
