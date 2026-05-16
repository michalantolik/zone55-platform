namespace BlogPlatform.Application.Roadmap;

public sealed class RoadmapQueryService : IRoadmapQueryService
{
    private readonly IDotnetRoadmapStore _roadmapStore;

    public RoadmapQueryService(IDotnetRoadmapStore roadmapStore)
    {
        _roadmapStore = roadmapStore;
    }

    public Task<BlogPlatform.Domain.Entities.DotnetRoadmap> GetRoadmapAsync(
        CancellationToken cancellationToken = default)
    {
        return _roadmapStore.GetAsync(cancellationToken);
    }

    public async Task<RoadmapOperationResult> ValidateStepAsync(
        string? zoneKey,
        string? stepKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(zoneKey))
        {
            return new RoadmapOperationResult(false, "Dotnet Zone is required.");
        }

        if (string.IsNullOrWhiteSpace(stepKey))
        {
            return new RoadmapOperationResult(false, "Dotnet Zone Step is required.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.ContainsZone(zoneKey))
        {
            return new RoadmapOperationResult(false, $"Invalid Dotnet Zone: {zoneKey}");
        }

        if (!roadmap.IsValidAssignment(zoneKey, stepKey))
        {
            return new RoadmapOperationResult(false, $"Dotnet Zone Step '{stepKey}' does not belong to Dotnet Zone '{zoneKey}'.");
        }

        return new RoadmapOperationResult(true, string.Empty);
    }
}
