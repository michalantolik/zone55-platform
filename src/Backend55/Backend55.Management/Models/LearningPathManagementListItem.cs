namespace Backend55.Management.Models;

public sealed record LearningPathManagementListItem(
    Guid Id,
    string Key,
    string Title,
    string Summary,
    int SortOrder);
