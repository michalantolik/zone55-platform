namespace LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningStep;

public sealed record UpdateLearningStepCommand(
    Guid LearningStepId,
    string Title,
    string? Summary);
