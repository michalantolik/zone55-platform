namespace BlogPlatform.Application.Roadmap;

public interface IRoadmapQueryService
{
    Task<IReadOnlyCollection<RoadmapZoneModel>> GetRoadmapAsync(
        CancellationToken cancellationToken = default);

    Task<RoadmapOperationResult> ValidateStepAsync(
        string? zoneKey,
        string? stepKey,
        CancellationToken cancellationToken = default);
}
