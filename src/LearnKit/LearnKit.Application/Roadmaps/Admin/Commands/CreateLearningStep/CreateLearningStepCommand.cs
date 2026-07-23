namespace LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningStep;

public sealed record CreateLearningStepCommand(Guid LearningZoneId, string Key, string Title, string? Summary);
