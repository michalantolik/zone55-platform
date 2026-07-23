using BlogPlatform.Api.Controllers.LearnKit.Admin.Models;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningPath;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningStep;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningZone;
using LearnKit.Application.Roadmaps.Admin.Queries.GetLearningPathForManagement;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers.LearnKit.Admin;

/// <summary>
/// Exposes LearnKit roadmap data required by management clients.
/// </summary>
[ApiController]
[Route("api/learnkit/admin/roadmaps")]
public sealed class RoadmapsManagementController : ControllerBase
{
    private readonly GetLearningPathForManagementHandler _getLearningPathHandler;
    private readonly UpdateLearningPathHandler _updateLearningPathHandler;
    private readonly UpdateLearningZoneHandler _updateLearningZoneHandler;
    private readonly UpdateLearningStepHandler _updateLearningStepHandler;

    /// <summary>
    /// Creates a new roadmap management controller.
    /// </summary>
    public RoadmapsManagementController(
        GetLearningPathForManagementHandler getLearningPathHandler,
        UpdateLearningPathHandler updateLearningPathHandler,
        UpdateLearningZoneHandler updateLearningZoneHandler,
        UpdateLearningStepHandler updateLearningStepHandler)
    {
        _getLearningPathHandler = getLearningPathHandler;
        _updateLearningPathHandler = updateLearningPathHandler;
        _updateLearningZoneHandler = updateLearningZoneHandler;
        _updateLearningStepHandler = updateLearningStepHandler;
    }

    /// <summary>
    /// Returns a learning path with its zones and steps for management.
    /// </summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(
        string key,
        CancellationToken cancellationToken)
    {
        var learningPath = await _getLearningPathHandler.HandleAsync(
            new GetLearningPathForManagementQuery(key),
            cancellationToken);

        return learningPath is null
            ? NotFound()
            : Ok(learningPath);
    }

    /// <summary>
    /// Updates the title and summary of a learning path.
    /// </summary>
    [HttpPut("paths/{learningPathId:guid}")]
    public async Task<IActionResult> UpdatePath(
        Guid learningPathId,
        UpdateLearningStructureItemRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _updateLearningPathHandler.HandleAsync(
            new UpdateLearningPathCommand(
                learningPathId,
                request.Title,
                request.Summary),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// Updates the title and summary of a learning zone.
    /// </summary>
    [HttpPut("zones/{learningZoneId:guid}")]
    public async Task<IActionResult> UpdateZone(
        Guid learningZoneId,
        UpdateLearningStructureItemRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _updateLearningZoneHandler.HandleAsync(
            new UpdateLearningZoneCommand(
                learningZoneId,
                request.Title,
                request.Summary),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// Updates the title and summary of a learning step.
    /// </summary>
    [HttpPut("steps/{learningStepId:guid}")]
    public async Task<IActionResult> UpdateStep(
        Guid learningStepId,
        UpdateLearningStructureItemRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _updateLearningStepHandler.HandleAsync(
            new UpdateLearningStepCommand(
                learningStepId,
                request.Title,
                request.Summary),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }
}
