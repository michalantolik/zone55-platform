namespace LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningPath;

public sealed record CreateLearningPathCommand(
    string Key,
    string Title,
    string? Summary);
