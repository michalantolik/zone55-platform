namespace BlogPlatform.Api.Controllers.LearnKit.Admin.Models;

/// <summary>
/// Basic article details accepted by the update endpoint.
/// </summary>
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
public sealed record UpdateArticleRequest(
    Guid LearningStepId,
    string Slug,
    string Title,
    string? Summary,
    int SortOrder);
