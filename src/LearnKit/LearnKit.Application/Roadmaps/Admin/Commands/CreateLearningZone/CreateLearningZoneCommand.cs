namespace LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningZone;

public sealed record CreateLearningZoneCommand(Guid LearningPathId, string Key, string Title, string? Summary);
