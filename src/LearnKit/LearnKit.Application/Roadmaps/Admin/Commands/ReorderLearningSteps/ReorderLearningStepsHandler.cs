using LearnKit.Application.Roadmaps.Admin.Contracts;
namespace LearnKit.Application.Roadmaps.Admin.Commands.ReorderLearningSteps;
public sealed class ReorderLearningStepsHandler(ILearningPathManagementStore store) { public async Task<bool> HandleAsync(ReorderLearningStepsCommand command,CancellationToken cancellationToken=default){ var zone=await store.GetTrackedZoneByIdAsync(command.LearningZoneId,cancellationToken); if(zone is null)return false; zone.ReorderSteps(command.OrderedStepIds); await store.SaveChangesAsync(cancellationToken); return true; } }
