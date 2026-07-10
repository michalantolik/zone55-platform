using LearnKit.Application.Articles.Public.Models;

namespace LearnKit.Application.Articles.Admin.Models;

/// <summary>
/// Represents article data required for management and editing.
/// </summary>
/// <param name="Id">
/// Unique article identifier.
/// </param>
/// <param name="LearningStepId">
/// Identifier of the learning step that owns the article.
/// </param>
/// <param name="Slug">
/// URL-friendly article identifier.
/// </param>
/// <param name="Title">
/// Article title.
/// </param>
/// <param name="Summary">
/// Short article description.
/// </param>
/// <param name="SortOrder">
/// Article position inside its learning step.
/// </param>
/// <param name="Status">
/// Current article publishing state.
/// </param>
/// <param name="Blocks">
/// Content blocks belonging to the article.
/// </param>
public sealed record ArticleManagementDetails(
    Guid Id,
    Guid LearningStepId,
    string Slug,
    string Title,
    string Summary,
    int SortOrder,
    string Status,
    IReadOnlyCollection<ArticleBlockDetails> Blocks);
