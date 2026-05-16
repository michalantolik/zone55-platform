using System.Text;

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
        var key = CreateSlug(string.IsNullOrWhiteSpace(requestedKey) ? name : requestedKey);

        if (roadmap.Zones.Any(zone => zone.Key == key))
        {
            return new RoadmapOperationResult(false, "Zone key already exists.");
        }

        roadmap.Zones.Add(new DotnetRoadmapZone
        {
            Key = key,
            Name = name.Trim(),
            Order = roadmap.Zones.Count + 1,
            Steps = []
        });

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
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == zoneKey);

        if (zone is null)
        {
            return new RoadmapOperationResult(false, "Zone not found.");
        }

        zone.Name = name.Trim();

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
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == zoneKey);

        if (zone is null)
        {
            return new RoadmapOperationResult(false, "Zone not found.");
        }

        roadmap.Zones.Remove(zone);
        ReorderZones(roadmap);

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
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == zoneKey);

        if (zone is null)
        {
            return new RoadmapOperationResult(false, "Zone not found.");
        }

        var key = CreateSlug(string.IsNullOrWhiteSpace(requestedKey) ? name : requestedKey);

        if (roadmap.Zones.SelectMany(item => item.Steps).Any(step => step.Key == key))
        {
            return new RoadmapOperationResult(false, "Step key already exists.");
        }

        zone.Steps.Add(new DotnetRoadmapStep
        {
            Key = key,
            Name = name.Trim(),
            Order = zone.Steps.Count + 1
        });

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
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == zoneKey);
        var step = zone?.Steps.FirstOrDefault(item => item.Key == stepKey);

        if (zone is null || step is null)
        {
            return new RoadmapOperationResult(false, "Step not found.");
        }

        step.Name = name.Trim();

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
        var zone = roadmap.Zones.FirstOrDefault(item => item.Key == zoneKey);
        var step = zone?.Steps.FirstOrDefault(item => item.Key == stepKey);

        if (zone is null || step is null)
        {
            return new RoadmapOperationResult(false, "Step not found.");
        }

        zone.Steps.Remove(step);
        ReorderSteps(zone);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return new RoadmapOperationResult(true, "Step deleted successfully.");
    }

    private static void ReorderZones(DotnetRoadmap roadmap)
    {
        var index = 1;

        foreach (var zone in roadmap.Zones.OrderBy(zone => zone.Order))
        {
            zone.Order = index++;
        }
    }

    private static void ReorderSteps(DotnetRoadmapZone zone)
    {
        var index = 1;

        foreach (var step in zone.Steps.OrderBy(step => step.Order))
        {
            step.Order = index++;
        }
    }

    private static string CreateSlug(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder();
        var lastWasDash = false;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                lastWasDash = false;
                continue;
            }

            if (!lastWasDash)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        return builder.ToString().Trim('-');
    }
}
