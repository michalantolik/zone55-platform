using BlogPlatform.Api.Authentication;
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
using LearnKit.Application.Roadmaps.Admin;
using LearnKit.Application.Roadmaps.Admin.Queries.GetLearningPathForManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers.LearnKit.Admin;

/// <summary>
/// Exposes LearnKit learning structure management endpoints.
/// </summary>
[ApiController]
[Authorize(Policy = LearnKitManagementAuthOptions.PolicyName)]
[Route("api/learnkit/admin/roadmaps")]
public sealed class RoadmapsManagementController(
    GetLearningPathForManagementHandler getPath,
    UpdateLearningPathHandler updatePath,
    UpdateLearningZoneHandler updateZone,
    UpdateLearningStepHandler updateStep,
    CreateLearningZoneHandler createZone,
    DeleteLearningZoneHandler deleteZone,
    ReorderLearningZonesHandler reorderZones,
    CreateLearningStepHandler createStep,
    DeleteLearningStepHandler deleteStep,
    ReorderLearningStepsHandler reorderSteps) : ControllerBase
{
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(
        string key,
        CancellationToken cancellationToken)
    {
        var path = await getPath.HandleAsync(
            new GetLearningPathForManagementQuery(key),
            cancellationToken);

        return path is null
            ? ManagementErrors.NotFound(
                "learning_path_not_found",
                "The requested learning path does not exist.")
            : Ok(path);
    }

    [HttpPut("paths/{id:guid}")]
    public async Task<IActionResult> UpdatePath(
        Guid id,
        UpdateLearningStructureItemRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await updatePath.HandleAsync(
            new UpdateLearningPathCommand(id, request.Title, request.Summary),
            cancellationToken);

        return updated
            ? NoContent()
            : ManagementErrors.NotFound(
                "learning_path_not_found",
                "The requested learning path does not exist.");
    }

    [HttpPut("zones/{id:guid}")]
    public async Task<IActionResult> UpdateZone(
        Guid id,
        UpdateLearningStructureItemRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await updateZone.HandleAsync(
            new UpdateLearningZoneCommand(id, request.Title, request.Summary),
            cancellationToken);

        return updated
            ? NoContent()
            : ManagementErrors.NotFound(
                "learning_zone_not_found",
                "The requested learning zone does not exist.");
    }

    [HttpPut("steps/{id:guid}")]
    public async Task<IActionResult> UpdateStep(
        Guid id,
        UpdateLearningStructureItemRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await updateStep.HandleAsync(
            new UpdateLearningStepCommand(id, request.Title, request.Summary),
            cancellationToken);

        return updated
            ? NoContent()
            : ManagementErrors.NotFound(
                "learning_step_not_found",
                "The requested learning step does not exist.");
    }

    [HttpPost("paths/{pathId:guid}/zones")]
    public async Task<IActionResult> CreateZone(
        Guid pathId,
        CreateLearningStructureItemRequest request,
        CancellationToken cancellationToken)
    {
        Guid? id;

        try
        {
            id = await createZone.HandleAsync(
                new CreateLearningZoneCommand(pathId, request.Key, request.Title, request.Summary),
                cancellationToken);
        }
        catch (LearningStructureKeyConflictException)
        {
            return ManagementErrors.Conflict(
                "learning_zone_key_conflict",
                "Another learning zone already uses this key.");
        }

        return id is null
            ? ManagementErrors.NotFound(
                "learning_path_not_found",
                "The requested learning path does not exist.")
            : Created(string.Empty, new { id });
    }

    [HttpDelete("paths/{pathId:guid}/zones/{zoneId:guid}")]
    public async Task<IActionResult> DeleteZone(
        Guid pathId,
        Guid zoneId,
        CancellationToken cancellationToken)
    {
        var result = await deleteZone.HandleAsync(
            new DeleteLearningZoneCommand(pathId, zoneId),
            cancellationToken);

        return MapDeleteResult(
            result,
            "learning_zone_not_found",
            "The requested learning zone does not exist.",
            "learning_zone_not_empty",
            "Remove the learning steps before deleting this zone.");
    }

    [HttpPut("paths/{pathId:guid}/zones/order")]
    public async Task<IActionResult> ReorderZones(
        Guid pathId,
        ReorderLearningStructureItemsRequest request,
        CancellationToken cancellationToken)
    {
        var reordered = await reorderZones.HandleAsync(
            new ReorderLearningZonesCommand(pathId, request.OrderedIds),
            cancellationToken);

        return reordered
            ? NoContent()
            : ManagementErrors.BadRequest(
                "learning_zone_order_invalid",
                "The zone order must contain every zone in the learning path exactly once.");
    }

    [HttpPost("zones/{zoneId:guid}/steps")]
    public async Task<IActionResult> CreateStep(
        Guid zoneId,
        CreateLearningStructureItemRequest request,
        CancellationToken cancellationToken)
    {
        Guid? id;

        try
        {
            id = await createStep.HandleAsync(
                new CreateLearningStepCommand(zoneId, request.Key, request.Title, request.Summary),
                cancellationToken);
        }
        catch (LearningStructureKeyConflictException)
        {
            return ManagementErrors.Conflict(
                "learning_step_key_conflict",
                "Another learning step already uses this key.");
        }

        return id is null
            ? ManagementErrors.NotFound(
                "learning_zone_not_found",
                "The requested learning zone does not exist.")
            : Created(string.Empty, new { id });
    }

    [HttpDelete("zones/{zoneId:guid}/steps/{stepId:guid}")]
    public async Task<IActionResult> DeleteStep(
        Guid zoneId,
        Guid stepId,
        CancellationToken cancellationToken)
    {
        var result = await deleteStep.HandleAsync(
            new DeleteLearningStepCommand(zoneId, stepId),
            cancellationToken);

        return MapDeleteResult(
            result,
            "learning_step_not_found",
            "The requested learning step does not exist.",
            "learning_step_not_empty",
            "Move or delete the articles before deleting this step.");
    }

    [HttpPut("zones/{zoneId:guid}/steps/order")]
    public async Task<IActionResult> ReorderSteps(
        Guid zoneId,
        ReorderLearningStructureItemsRequest request,
        CancellationToken cancellationToken)
    {
        var reordered = await reorderSteps.HandleAsync(
            new ReorderLearningStepsCommand(zoneId, request.OrderedIds),
            cancellationToken);

        return reordered
            ? NoContent()
            : ManagementErrors.BadRequest(
                "learning_step_order_invalid",
                "The step order must contain every step in the learning zone exactly once.");
    }

    private IActionResult MapDeleteResult(
        LearningStructureOperationResult result,
        string notFoundCode,
        string notFoundMessage,
        string conflictCode,
        string conflictMessage) =>
        result switch
        {
            LearningStructureOperationResult.Success => NoContent(),
            LearningStructureOperationResult.NotFound =>
                ManagementErrors.NotFound(notFoundCode, notFoundMessage),
            LearningStructureOperationResult.Conflict =>
                ManagementErrors.Conflict(conflictCode, conflictMessage),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
}
