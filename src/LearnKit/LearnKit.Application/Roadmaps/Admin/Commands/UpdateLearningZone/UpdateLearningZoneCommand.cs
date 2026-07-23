namespace LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningZone;

public sealed record UpdateLearningZoneCommand(
    Guid LearningZoneId,
    string Title,
    string? Summary);
