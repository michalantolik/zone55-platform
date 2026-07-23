namespace LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningPath;

public sealed record UpdateLearningPathCommand(
    Guid LearningPathId,
    string Title,
    string? Summary);
