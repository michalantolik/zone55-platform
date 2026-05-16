using BlogPlatform.Domain.ValueObjects;

namespace BlogPlatform.Domain.Entities;

public sealed class DotnetRoadmap
{
    public List<DotnetRoadmapZone> Zones { get; set; } = [];

    public DotnetRoadmapZone? FindZone(string? zoneKey)
    {
        return Zones.FirstOrDefault(zone =>
            string.Equals(zone.Key, zoneKey?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool ContainsZone(string zoneKey)
    {
        return FindZone(zoneKey) is not null;
    }

    public bool ContainsStep(string stepKey)
    {
        return Zones
            .SelectMany(zone => zone.Steps)
            .Any(step => string.Equals(step.Key, stepKey, StringComparison.OrdinalIgnoreCase));
    }

    public void AddZone(string name, string? requestedKey)
    {
        var key = CreateKey(string.IsNullOrWhiteSpace(requestedKey) ? name : requestedKey);

        Zones.Add(new DotnetRoadmapZone
        {
            Key = key,
            Name = name.Trim(),
            Order = Zones.Count + 1,
            Steps = []
        });
    }

    public void RenameZone(string zoneKey, string name)
    {
        var zone = FindZone(zoneKey)
            ?? throw new InvalidOperationException("Zone not found.");

        zone.Name = name.Trim();
    }

    public void DeleteZone(string zoneKey)
    {
        var zone = FindZone(zoneKey)
            ?? throw new InvalidOperationException("Zone not found.");

        Zones.Remove(zone);
        ReorderZones();
    }

    public void AddStep(string zoneKey, string name, string? requestedKey)
    {
        var zone = FindZone(zoneKey)
            ?? throw new InvalidOperationException("Zone not found.");

        var key = CreateKey(string.IsNullOrWhiteSpace(requestedKey) ? name : requestedKey);

        zone.Steps.Add(new DotnetRoadmapStep
        {
            Key = key,
            Name = name.Trim(),
            Order = zone.Steps.Count + 1
        });
    }

    public void RenameStep(string zoneKey, string stepKey, string name)
    {
        var step = FindStep(zoneKey, stepKey)
            ?? throw new InvalidOperationException("Step not found.");

        step.Name = name.Trim();
    }

    public void DeleteStep(string zoneKey, string stepKey)
    {
        var zone = FindZone(zoneKey)
            ?? throw new InvalidOperationException("Zone not found.");

        var step = zone.FindStep(stepKey)
            ?? throw new InvalidOperationException("Step not found.");

        zone.Steps.Remove(step);
        zone.ReorderSteps();
    }

    public bool IsValidAssignment(string? zoneKey, string? stepKey)
    {
        var zone = FindZone(zoneKey);

        return zone is not null && zone.ContainsStep(stepKey);
    }

    private void ReorderZones()
    {
        var index = 1;

        foreach (var zone in Zones.OrderBy(zone => zone.Order))
        {
            zone.Order = index++;
        }
    }

    private DotnetRoadmapStep? FindStep(string zoneKey, string stepKey)
    {
        return FindZone(zoneKey)?.FindStep(stepKey);
    }

    private static string CreateKey(string value)
    {
        return Slug.Create(value).Value;
    }
}

public sealed class DotnetRoadmapZone
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<DotnetRoadmapStep> Steps { get; set; } = [];

    public DotnetRoadmapStep? FindStep(string? stepKey)
    {
        return Steps.FirstOrDefault(step =>
            string.Equals(step.Key, stepKey?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool ContainsStep(string? stepKey)
    {
        return FindStep(stepKey) is not null;
    }

    public void ReorderSteps()
    {
        var index = 1;

        foreach (var step in Steps.OrderBy(step => step.Order))
        {
            step.Order = index++;
        }
    }
}

public sealed class DotnetRoadmapStep
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }
}
