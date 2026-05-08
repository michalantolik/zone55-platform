namespace BlogPlatform.App.Models;

public sealed record LearningPathLevel(
    int Number,
    string Title,
    string Description,
    string AccentClass,
    IReadOnlyCollection<LearningPathStep> Steps);
