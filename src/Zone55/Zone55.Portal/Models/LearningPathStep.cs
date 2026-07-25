namespace Zone55.Portal.Models;

public sealed record LearningPathStep(
    int GlobalOrder,
    int StepOrder,
    string Key,
    string Title,
    string Description,
    string Difficulty,
    string Icon,
    IReadOnlyCollection<string> Keywords,
    IReadOnlyCollection<ArticleListItem> Articles);
