using LearnKit.Application.Roadmaps.Queries.GetLearningPath;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers.LearnKit;

/// <summary>
/// Provides access to learning paths.
/// </summary>
[ApiController]
[Route("api/learnkit/roadmaps")]
public sealed class RoadmapsController : ControllerBase
{
    private readonly GetLearningPathHandler _handler;

    public RoadmapsController(GetLearningPathHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Gets a learning path by its key.
    /// </summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(
        string key,
        CancellationToken cancellationToken)
    {
        var roadmap = await _handler.HandleAsync(
            new GetLearningPathQuery(key),
            cancellationToken);

        if (roadmap is null)
        {
            return NotFound();
        }

        return Ok(roadmap);
    }
}
