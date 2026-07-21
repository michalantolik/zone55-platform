namespace LearnKit.Application.Roadmaps.Admin.Models;

public sealed record LearningPathManagementDetails(
    Guid Id,
    string Key,
    string Title,
    IReadOnlyCollection<LearningZoneManagementDetails> Zones);

public sealed record LearningZoneManagementDetails(
    Guid Id,
    string Key,
    string Title,
    int SortOrder,
    IReadOnlyCollection<LearningStepManagementDetails> Steps);

public sealed record LearningStepManagementDetails(
    Guid Id,
    string Key,
    string Title,
    int SortOrder);
