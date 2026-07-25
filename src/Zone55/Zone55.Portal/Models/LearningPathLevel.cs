namespace Zone55.Portal.Models;

public sealed record LearningPathLevel(
    string Key,
    int Number,
    string Title,
    string Description,
    string AccentClass,
    IReadOnlyCollection<LearningPathStep> Steps);
