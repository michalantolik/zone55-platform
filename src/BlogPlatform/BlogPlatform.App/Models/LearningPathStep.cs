namespace BlogPlatform.App.Models;

public sealed record LearningPathStep(
    int GlobalOrder,
    int StepOrder,
    string Key,
    string Title,
    string Description,
    string Difficulty,
    string Icon,
    IReadOnlyCollection<string> Keywords,
    IReadOnlyCollection<PostListItem> Posts);
