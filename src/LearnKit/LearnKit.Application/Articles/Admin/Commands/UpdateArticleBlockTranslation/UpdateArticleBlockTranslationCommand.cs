namespace LearnKit.Application.Articles.Admin.Commands.UpdateArticleBlockTranslation;

/// <summary>
/// Represents a request to create or update
/// one language version of an article block.
/// </summary>
/// <param name="ArticleId">
/// Article owning the block.
/// </param>
/// <param name="BlockId">
/// Block being translated.
/// </param>
/// <param name="LanguageCode">
/// Language version being edited.
/// </param>
/// <param name="ContentJson">
/// Translated block content.
/// </param>
public sealed record UpdateArticleBlockTranslationCommand(
    Guid ArticleId,
    Guid BlockId,
    string LanguageCode,
    string ContentJson);
