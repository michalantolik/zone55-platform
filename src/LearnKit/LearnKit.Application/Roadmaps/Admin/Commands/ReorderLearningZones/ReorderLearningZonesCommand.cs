namespace LearnKit.Application.Roadmaps.Admin.Commands.ReorderLearningZones;

public sealed record ReorderLearningZonesCommand(Guid LearningPathId, IReadOnlyCollection<Guid> OrderedZoneIds);
