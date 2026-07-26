namespace LearnKit.Application.Articles.Admin.Commands.UpdateArticleTranslation;

/// <summary>
/// Represents a request to create or update
/// one language version of an article.
/// </summary>
/// <param name="ArticleId">
/// Article being translated.
/// </param>
/// <param name="LanguageCode">
/// Language version being edited.
/// </param>
/// <param name="Title">
/// Translated article title.
/// </param>
/// <param name="Summary">
/// Translated article summary.
/// </param>
public sealed record UpdateArticleTranslationCommand(
    Guid ArticleId,
    string LanguageCode,
    string Title,
    string? Summary);
