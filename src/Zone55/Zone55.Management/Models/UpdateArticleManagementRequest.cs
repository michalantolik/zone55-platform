namespace Zone55.Management.Models;

public sealed record UpdateArticleManagementRequest(
    Guid LearningStepId,
    string Slug,
    string Title,
    string? Summary,
    int SortOrder);
