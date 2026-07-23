namespace LearnKit.Application.Roadmaps.Admin.Commands.ReorderLearningSteps;

public sealed record ReorderLearningStepsCommand(Guid LearningZoneId, IReadOnlyCollection<Guid> OrderedStepIds);
