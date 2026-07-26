using LearnKit.Application.Articles.Public.Models;

namespace LearnKit.Application.Articles.Admin.Models;

/// <summary>
/// Represents article data required for management and editing.
/// </summary>
/// <param name="Id">
/// Unique article identifier.
/// </param>
/// <param name="LearningStepId">
/// Identifier of the learning step owning the article.
/// </param>
/// <param name="Slug">
/// Slug shared by every language version.
/// </param>
/// <param name="Title">
/// Title in the selected language.
/// </param>
/// <param name="Summary">
/// Summary in the selected language.
/// </param>
/// <param name="SortOrder">
/// Article position inside its learning step.
/// </param>
/// <param name="Status">
/// Publishing state of the selected language.
/// </param>
/// <param name="Blocks">
/// Blocks prepared in the selected language.
/// </param>
/// <param name="LanguageCode">
/// Language selected for editing.
/// </param>
/// <param name="TranslationExists">
/// Indicates whether the selected translation already exists.
/// </param>
/// <param name="IsFallback">
/// Indicates that default-language content is shown
/// because the selected translation does not exist.
/// </param>
/// <param name="AvailableLanguages">
/// Languages currently available for this article.
/// </param>
public sealed record ArticleManagementDetails(
    Guid Id,
    Guid LearningStepId,
    string Slug,
    string Title,
    string Summary,
    int SortOrder,
    string Status,
    IReadOnlyCollection<ArticleBlockDetails> Blocks,
    string LanguageCode = "en",
    bool TranslationExists = true,
    bool IsFallback = false,
    IReadOnlyCollection<string>? AvailableLanguages = null);
