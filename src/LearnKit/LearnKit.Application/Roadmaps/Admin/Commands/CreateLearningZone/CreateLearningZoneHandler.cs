using LearnKit.Application.Roadmaps.Admin.Contracts;
using LearnKit.Domain.Roadmaps;
namespace LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningZone;
public sealed class CreateLearningZoneHandler(ILearningPathManagementStore store)
{
 public async Task<Guid?> HandleAsync(CreateLearningZoneCommand command, CancellationToken cancellationToken=default)
 {
  var path=await store.GetTrackedPathByIdAsync(command.LearningPathId,cancellationToken); if(path is null) return null;
  var zone=new LearningZone(command.Key,command.Title,command.Summary,path.Zones.Count+1); path.AddZone(zone); await store.SaveChangesAsync(cancellationToken); return zone.Id;
 }
}
