using LearnKit.Application.Roadmaps.Admin.Contracts;
using LearnKit.Domain.Roadmaps;
namespace LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningStep;
public sealed class CreateLearningStepHandler(ILearningPathManagementStore store)
{
 public async Task<Guid?> HandleAsync(CreateLearningStepCommand command, CancellationToken cancellationToken=default)
 {
  var zone=await store.GetTrackedZoneByIdAsync(command.LearningZoneId,cancellationToken); if(zone is null) return null;
  var step=new LearningStep(command.Key,command.Title,command.Summary,zone.Steps.Count+1); zone.AddStep(step); await store.SaveChangesAsync(cancellationToken); return step.Id;
 }
}
