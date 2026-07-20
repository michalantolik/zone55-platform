namespace LearnKit.Application.Articles.Admin.Commands.CreateArticle;

/// <summary>
/// Represents a request to create a draft article.
/// </summary>
/// <param name="LearningStepId">
/// Learning step that owns the article.
/// </param>
/// <param name="Slug">
/// URL-friendly article identifier.
/// </param>
/// <param name="Title">
/// Article title shown to the user.
/// </param>
/// <param name="Summary">
/// Short article description.
/// </param>
/// <param name="SortOrder">
/// Position of the article inside its learning step.
/// </param>
public sealed record CreateArticleCommand(
    Guid LearningStepId,
    string Slug,
    string Title,
    string? Summary,
    int SortOrder);
