namespace BlogPlatform.Application.Roadmap;

public sealed class RoadmapCommandService : IRoadmapCommandService
{
    private readonly IDotnetRoadmapStore _roadmapStore;

    public RoadmapCommandService(IDotnetRoadmapStore roadmapStore)
    {
        _roadmapStore = roadmapStore;
    }

    public async Task<RoadmapOperationResult> CreateZoneAsync(
        string? name,
        string? requestedKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new RoadmapOperationResult(false, "Zone name is required.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);
        var key = CreateComparisonKey(requestedKey ?? name);

        if (roadmap.ContainsZone(key))
        {
            return new RoadmapOperationResult(false, "Zone key already exists.");
        }

        roadmap.AddZone(name, requestedKey);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return new RoadmapOperationResult(true, "Zone created successfully.");
    }

    public async Task<RoadmapOperationResult> UpdateZoneAsync(
        string zoneKey,
        string? name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new RoadmapOperationResult(false, "Zone name is required.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.ContainsZone(zoneKey))
        {
            return new RoadmapOperationResult(false, "Zone not found.");
        }

        roadmap.RenameZone(zoneKey, name);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return new RoadmapOperationResult(true, "Zone updated successfully.");
    }

    public async Task<RoadmapOperationResult> DeleteZoneAsync(
        string zoneKey,
        bool hasAssignedArticles,
        CancellationToken cancellationToken = default)
    {
        if (hasAssignedArticles)
        {
            return new RoadmapOperationResult(false, "Cannot delete zone because articles still use it.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.ContainsZone(zoneKey))
        {
            return new RoadmapOperationResult(false, "Zone not found.");
        }

        roadmap.DeleteZone(zoneKey);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return new RoadmapOperationResult(true, "Zone deleted successfully.");
    }

    public async Task<RoadmapOperationResult> CreateStepAsync(
        string zoneKey,
        string? name,
        string? requestedKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new RoadmapOperationResult(false, "Step name is required.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.ContainsZone(zoneKey))
        {
            return new RoadmapOperationResult(false, "Zone not found.");
        }

        var key = CreateComparisonKey(requestedKey ?? name);

        if (roadmap.ContainsStep(key))
        {
            return new RoadmapOperationResult(false, "Step key already exists.");
        }

        roadmap.AddStep(zoneKey, name, requestedKey);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return new RoadmapOperationResult(true, "Step created successfully.");
    }

    public async Task<RoadmapOperationResult> UpdateStepAsync(
        string zoneKey,
        string stepKey,
        string? name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new RoadmapOperationResult(false, "Step name is required.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.IsValidAssignment(zoneKey, stepKey))
        {
            return new RoadmapOperationResult(false, "Step not found.");
        }

        roadmap.RenameStep(zoneKey, stepKey, name);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return new RoadmapOperationResult(true, "Step updated successfully.");
    }

    public async Task<RoadmapOperationResult> DeleteStepAsync(
        string zoneKey,
        string stepKey,
        bool hasAssignedArticles,
        CancellationToken cancellationToken = default)
    {
        if (hasAssignedArticles)
        {
            return new RoadmapOperationResult(false, "Cannot delete step because articles still use it.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.IsValidAssignment(zoneKey, stepKey))
        {
            return new RoadmapOperationResult(false, "Step not found.");
        }

        roadmap.DeleteStep(zoneKey, stepKey);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return new RoadmapOperationResult(true, "Step deleted successfully.");
    }

    private static string CreateComparisonKey(string value)
    {
        return BlogPlatform.Domain.ValueObjects.Slug.Create(value).Value;
    }
}
