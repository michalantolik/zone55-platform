using LearnKit.Application.Roadmaps.Public.Queries.GetLearningPath;
using LearnKit.Application.Roadmaps.Public.Queries.GetFirstLearningPath;
using Microsoft.AspNetCore.Mvc;

namespace Zone55.Api.Controllers.LearnKit.Public;

/// <summary>
/// Provides access to learning paths.
/// </summary>
[ApiController]
[Route("api/learnkit/roadmaps")]
public sealed class RoadmapsController : ControllerBase
{
    private readonly GetLearningPathHandler _handler;
    private readonly GetFirstLearningPathHandler _firstHandler;

    public RoadmapsController(
        GetLearningPathHandler handler,
        GetFirstLearningPathHandler firstHandler)
    {
        _handler = handler;
        _firstHandler = firstHandler;
    }

    /// <summary>
    /// Gets the first learning path by sort order.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFirst(CancellationToken cancellationToken)
    {
        var roadmap = await _firstHandler.HandleAsync(
            new GetFirstLearningPathQuery(),
            cancellationToken);

        return roadmap is null ? NotFound() : Ok(roadmap);
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
