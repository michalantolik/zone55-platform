using BlogPlatform.Api.Authentication;
using Microsoft.AspNetCore.Authorization;
using BlogPlatform.Api.Controllers.LearnKit.Admin.Models;
using LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningStep;
using LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningZone;
using LearnKit.Application.Roadmaps.Admin.Commands.DeleteLearningStep;
using LearnKit.Application.Roadmaps.Admin.Commands.DeleteLearningZone;
using LearnKit.Application.Roadmaps.Admin.Commands.ReorderLearningSteps;
using LearnKit.Application.Roadmaps.Admin.Commands.ReorderLearningZones;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningPath;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningStep;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningZone;
using LearnKit.Application.Roadmaps.Admin.Models;
using LearnKit.Application.Roadmaps.Admin.Queries.GetLearningPathForManagement;
using Microsoft.AspNetCore.Mvc;
namespace BlogPlatform.Api.Controllers.LearnKit.Admin;
[ApiController]
[Authorize(Policy = LearnKitManagementAuthOptions.PolicyName)]
[Route("api/learnkit/admin/roadmaps")]
public sealed class RoadmapsManagementController(
 GetLearningPathForManagementHandler getPath, UpdateLearningPathHandler updatePath, UpdateLearningZoneHandler updateZone, UpdateLearningStepHandler updateStep,
 CreateLearningZoneHandler createZone, DeleteLearningZoneHandler deleteZone, ReorderLearningZonesHandler reorderZones,
 CreateLearningStepHandler createStep, DeleteLearningStepHandler deleteStep, ReorderLearningStepsHandler reorderSteps) : ControllerBase
{
 [HttpGet("{key}")] public async Task<IActionResult> GetByKey(string key,CancellationToken ct){var x=await getPath.HandleAsync(new(key),ct);return x is null?NotFound():Ok(x);}
 [HttpPut("paths/{id:guid}")] public async Task<IActionResult> UpdatePath(Guid id,UpdateLearningStructureItemRequest r,CancellationToken ct)=>await updatePath.HandleAsync(new(id,r.Title,r.Summary),ct)?NoContent():NotFound();
 [HttpPut("zones/{id:guid}")] public async Task<IActionResult> UpdateZone(Guid id,UpdateLearningStructureItemRequest r,CancellationToken ct)=>await updateZone.HandleAsync(new(id,r.Title,r.Summary),ct)?NoContent():NotFound();
 [HttpPut("steps/{id:guid}")] public async Task<IActionResult> UpdateStep(Guid id,UpdateLearningStructureItemRequest r,CancellationToken ct)=>await updateStep.HandleAsync(new(id,r.Title,r.Summary),ct)?NoContent():NotFound();
 [HttpPost("paths/{pathId:guid}/zones")] public async Task<IActionResult> CreateZone(Guid pathId,CreateLearningStructureItemRequest r,CancellationToken ct){var id=await createZone.HandleAsync(new(pathId,r.Key,r.Title,r.Summary),ct);return id is null?NotFound():Created(string.Empty,new{id});}
 [HttpDelete("paths/{pathId:guid}/zones/{zoneId:guid}")] public async Task<IActionResult> DeleteZone(Guid pathId,Guid zoneId,CancellationToken ct)=>Map(await deleteZone.HandleAsync(new(pathId,zoneId),ct),"Remove the learning steps before deleting this zone.");
 [HttpPut("paths/{pathId:guid}/zones/order")] public async Task<IActionResult> ReorderZones(Guid pathId,ReorderLearningStructureItemsRequest r,CancellationToken ct)=>await reorderZones.HandleAsync(new(pathId,r.OrderedIds),ct)?NoContent():NotFound();
 [HttpPost("zones/{zoneId:guid}/steps")] public async Task<IActionResult> CreateStep(Guid zoneId,CreateLearningStructureItemRequest r,CancellationToken ct){var id=await createStep.HandleAsync(new(zoneId,r.Key,r.Title,r.Summary),ct);return id is null?NotFound():Created(string.Empty,new{id});}
 [HttpDelete("zones/{zoneId:guid}/steps/{stepId:guid}")] public async Task<IActionResult> DeleteStep(Guid zoneId,Guid stepId,CancellationToken ct)=>Map(await deleteStep.HandleAsync(new(zoneId,stepId),ct),"Move or delete the articles before deleting this step.");
 [HttpPut("zones/{zoneId:guid}/steps/order")] public async Task<IActionResult> ReorderSteps(Guid zoneId,ReorderLearningStructureItemsRequest r,CancellationToken ct)=>await reorderSteps.HandleAsync(new(zoneId,r.OrderedIds),ct)?NoContent():NotFound();
 private IActionResult Map(LearningStructureOperationResult result,string conflict)=>result switch { LearningStructureOperationResult.Success=>NoContent(), LearningStructureOperationResult.NotFound=>NotFound(), LearningStructureOperationResult.Conflict=>Conflict(new ProblemDetails{Title="The structure item is not empty.",Detail=conflict,Status=409}), _=>StatusCode(500)};
}
