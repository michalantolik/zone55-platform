namespace BlogPlatform.Api.Controllers.LearnKit.Admin.Models;

/// <summary>
/// Defines the complete article order for one learning step.
/// </summary>
public sealed record ReorderArticlesRequest(
    Guid LearningStepId,
    IReadOnlyCollection<Guid> OrderedArticleIds);
