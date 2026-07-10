namespace LearnKit.Application.Articles.Admin.Models;

/// <summary>
/// Represents one article displayed on the article management list.
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
/// Article title displayed in the administration panel.
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
/// <param name="BlockCount">
/// Number of content blocks contained in the article.
/// </param>
public sealed record ArticleManagementListItem(
    Guid Id,
    Guid LearningStepId,
    string Slug,
    string Title,
    string Summary,
    int SortOrder,
    string Status,
    int BlockCount);
