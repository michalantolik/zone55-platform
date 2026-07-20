namespace LearnKit.Application.Articles.Admin.Commands.UpdateArticle;

/// <summary>
/// Represents a request to update basic article details.
/// </summary>
/// <param name="ArticleId">
/// Unique article identifier.
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
public sealed record UpdateArticleCommand(
    Guid ArticleId,
    string Slug,
    string Title,
    string? Summary,
    int SortOrder);
