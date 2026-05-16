using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Roadmap;

public interface IRoadmapQueryService
{
    Task<DotnetRoadmap> GetRoadmapAsync(
        CancellationToken cancellationToken = default);

    Task<RoadmapOperationResult> ValidateStepAsync(
        string? zoneKey,
        string? stepKey,
        CancellationToken cancellationToken = default);
}
