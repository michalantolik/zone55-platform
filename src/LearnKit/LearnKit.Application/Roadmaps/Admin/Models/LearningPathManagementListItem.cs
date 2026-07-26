namespace LearnKit.Application.Roadmaps.Admin.Models;

public sealed record LearningPathManagementListItem(
    Guid Id,
    string Key,
    string Title,
    string Summary,
    int SortOrder);
