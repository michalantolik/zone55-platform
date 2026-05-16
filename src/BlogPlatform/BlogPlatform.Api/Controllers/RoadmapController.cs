using BlogPlatform.Api.Mapping;
using BlogPlatform.Application.Roadmap;
using BlogPlatform.Contracts.DotnetRoadmap;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/roadmap")]
public sealed class RoadmapController : ControllerBase
{
    private readonly IRoadmapQueryService _roadmapQueries;

    public RoadmapController(IRoadmapQueryService roadmapQueries)
    {
        _roadmapQueries = roadmapQueries;
    }

    [HttpGet("dotnet")]
    public async Task<ActionResult<IReadOnlyCollection<RoadmapZoneDto>>> GetDotnetRoadmap(
        CancellationToken cancellationToken)
    {
        var roadmap = await _roadmapQueries.GetRoadmapAsync(cancellationToken);

        return Ok(
            roadmap
                .OrderBy(zone => zone.Order)
                .Select(RoadmapContractMapper.ToDto)
                .ToList());
    }
}
