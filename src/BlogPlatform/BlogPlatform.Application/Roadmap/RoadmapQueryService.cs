namespace BlogPlatform.Application.Roadmap;

public sealed class RoadmapQueryService : IRoadmapQueryService
{
    private readonly IDotnetRoadmapStore _roadmapStore;

    public RoadmapQueryService(IDotnetRoadmapStore roadmapStore)
    {
        _roadmapStore = roadmapStore;
    }

    public async Task<IReadOnlyCollection<RoadmapZoneModel>> GetRoadmapAsync(
        CancellationToken cancellationToken = default)
    {
        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        return roadmap.Zones
            .OrderBy(zone => zone.Order)
            .Select(zone => new RoadmapZoneModel(
                zone.Key,
                zone.Name,
                zone.Order,
                zone.Steps
                    .OrderBy(step => step.Order)
                    .Select(step => new RoadmapStepModel(
                        step.Key,
                        step.Name,
                        step.Order))
                    .ToList()))
            .ToList();
    }

    public async Task<RoadmapOperationResult> ValidateStepAsync(
        string? zoneKey,
        string? stepKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(zoneKey))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.ZoneRequired,
                "Dotnet Zone is required.");
        }

        if (string.IsNullOrWhiteSpace(stepKey))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.StepRequired,
                "Dotnet Zone Step is required.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.ContainsZone(zoneKey))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.ZoneNotFound,
                $"Invalid Dotnet Zone: {zoneKey}");
        }

        if (!roadmap.IsValidAssignment(zoneKey, stepKey))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.StepDoesNotBelongToZone,
                $"Dotnet Zone Step '{stepKey}' does not belong to Dotnet Zone '{zoneKey}'.");
        }

        return RoadmapOperationResult.Ok(string.Empty);
    }
}
