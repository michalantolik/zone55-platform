namespace Zone55.Management.Models;

public sealed record CreateArticleManagementRequest(
    Guid LearningStepId,
    string Slug,
    string Title,
    string? Summary,
    int SortOrder);
