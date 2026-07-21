namespace Zone55.Management.Models;

public sealed record ArticleManagementListItem(
    Guid Id,
    Guid LearningStepId,
    string Slug,
    string Title,
    string Summary,
    int SortOrder,
    string Status,
    int BlockCount);
