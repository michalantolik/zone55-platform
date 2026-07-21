using LearnKit.Application.Roadmaps.Admin.Queries.GetLearningPathForManagement;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers.LearnKit.Admin;

/// <summary>
/// Exposes LearnKit roadmap data required by article management clients.
/// </summary>
[ApiController]
[Route("api/learnkit/admin/roadmaps")]
public sealed class RoadmapsManagementController : ControllerBase
{
    private readonly GetLearningPathForManagementHandler _getLearningPathHandler;

    /// <summary>
    /// Creates a new roadmap management controller.
    /// </summary>
    public RoadmapsManagementController(
        GetLearningPathForManagementHandler getLearningPathHandler)
    {
        _getLearningPathHandler = getLearningPathHandler;
    }

    /// <summary>
    /// Returns a learning path with its zones and steps for management.
    /// </summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(
        string key,
        CancellationToken cancellationToken)
    {
        var query = new GetLearningPathForManagementQuery(key);

        var learningPath = await _getLearningPathHandler.HandleAsync(
            query,
            cancellationToken);

        if (learningPath is null)
        {
            return NotFound();
        }

        return Ok(learningPath);
    }
}
