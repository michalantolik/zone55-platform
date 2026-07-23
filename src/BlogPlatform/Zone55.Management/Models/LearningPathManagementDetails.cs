namespace Zone55.Management.Models;

public sealed record LearningPathManagementDetails(
    Guid Id,
    string Key,
    string Title,
    string Summary,
    IReadOnlyCollection<LearningZoneManagementDetails> Zones);

public sealed record LearningZoneManagementDetails(
    Guid Id,
    string Key,
    string Title,
    string Summary,
    int SortOrder,
    IReadOnlyCollection<LearningStepManagementDetails> Steps);

public sealed record LearningStepManagementDetails(
    Guid Id,
    string Key,
    string Title,
    string Summary,
    int SortOrder);
