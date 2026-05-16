using BlogPlatform.Domain.ValueObjects;

namespace BlogPlatform.Domain.Entities;

public sealed class DotnetRoadmap
{
    private readonly List<DotnetRoadmapZone> _zones = [];

    public IReadOnlyCollection<DotnetRoadmapZone> Zones =>
        _zones.OrderBy(zone => zone.Order).ToList();

    public static DotnetRoadmap Create(IEnumerable<DotnetRoadmapZone> zones)
    {
        var roadmap = new DotnetRoadmap();

        foreach (var zone in zones.OrderBy(zone => zone.Order))
        {
            roadmap._zones.Add(zone);
        }

        roadmap.ReorderZones();

        return roadmap;
    }

    public DotnetRoadmapZone? FindZone(string? zoneKey)
    {
        return _zones.FirstOrDefault(zone =>
            string.Equals(zone.Key, zoneKey?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool ContainsZone(string? zoneKey)
    {
        return FindZone(zoneKey) is not null;
    }

    public bool ContainsStep(string? stepKey)
    {
        return _zones
            .SelectMany(zone => zone.Steps)
            .Any(step => string.Equals(step.Key, stepKey?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public void AddZone(string name, string? requestedKey)
    {
        EnsureRequired(name, "Zone name is required.");

        var key = CreateKey(string.IsNullOrWhiteSpace(requestedKey) ? name : requestedKey);

        if (ContainsZone(key))
        {
            throw new InvalidOperationException("Zone key already exists.");
        }

        _zones.Add(DotnetRoadmapZone.Create(
            key,
            name.Trim(),
            _zones.Count + 1,
            []));
    }

    public void RenameZone(string zoneKey, string name)
    {
        EnsureRequired(name, "Zone name is required.");

        var zone = FindZone(zoneKey)
            ?? throw new InvalidOperationException("Zone not found.");

        zone.Rename(name);
    }

    public void DeleteZone(string zoneKey)
    {
        var zone = FindZone(zoneKey)
            ?? throw new InvalidOperationException("Zone not found.");

        _zones.Remove(zone);
        ReorderZones();
    }

    public void AddStep(string zoneKey, string name, string? requestedKey)
    {
        EnsureRequired(name, "Step name is required.");

        var zone = FindZone(zoneKey)
            ?? throw new InvalidOperationException("Zone not found.");

        var key = CreateKey(string.IsNullOrWhiteSpace(requestedKey) ? name : requestedKey);

        if (ContainsStep(key))
        {
            throw new InvalidOperationException("Step key already exists.");
        }

        zone.AddStep(key, name);
    }

    public void RenameStep(string zoneKey, string stepKey, string name)
    {
        EnsureRequired(name, "Step name is required.");

        var step = FindStep(zoneKey, stepKey)
            ?? throw new InvalidOperationException("Step not found.");

        step.Rename(name);
    }

    public void DeleteStep(string zoneKey, string stepKey)
    {
        var zone = FindZone(zoneKey)
            ?? throw new InvalidOperationException("Zone not found.");

        zone.DeleteStep(stepKey);
    }

    public bool IsValidAssignment(string? zoneKey, string? stepKey)
    {
        var zone = FindZone(zoneKey);

        return zone is not null && zone.ContainsStep(stepKey);
    }

    private void ReorderZones()
    {
        var index = 1;

        foreach (var zone in _zones.OrderBy(zone => zone.Order))
        {
            zone.SetOrder(index++);
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

    private static void EnsureRequired(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message);
        }
    }
}

public sealed class DotnetRoadmapZone
{
    private readonly List<DotnetRoadmapStep> _steps = [];

    private DotnetRoadmapZone(
        string key,
        string name,
        int order,
        IEnumerable<DotnetRoadmapStep> steps)
    {
        Key = key;
        Name = name;
        Order = order;
        _steps.AddRange(steps.OrderBy(step => step.Order));
        ReorderSteps();
    }

    public string Key { get; }

    public string Name { get; private set; }

    public int Order { get; private set; }

    public IReadOnlyCollection<DotnetRoadmapStep> Steps =>
        _steps.OrderBy(step => step.Order).ToList();

    public static DotnetRoadmapZone Create(
        string key,
        string name,
        int order,
        IEnumerable<DotnetRoadmapStep> steps)
    {
        return new DotnetRoadmapZone(
            key.Trim(),
            name.Trim(),
            order,
            steps);
    }

    public DotnetRoadmapStep? FindStep(string? stepKey)
    {
        return _steps.FirstOrDefault(step =>
            string.Equals(step.Key, stepKey?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool ContainsStep(string? stepKey)
    {
        return FindStep(stepKey) is not null;
    }

    public void AddStep(string key, string name)
    {
        _steps.Add(DotnetRoadmapStep.Create(
            key,
            name.Trim(),
            _steps.Count + 1));
    }

    public void DeleteStep(string stepKey)
    {
        var step = FindStep(stepKey)
            ?? throw new InvalidOperationException("Step not found.");

        _steps.Remove(step);
        ReorderSteps();
    }

    public void Rename(string name)
    {
        Name = name.Trim();
    }

    public void SetOrder(int order)
    {
        Order = order;
    }

    public void ReorderSteps()
    {
        var index = 1;

        foreach (var step in _steps.OrderBy(step => step.Order))
        {
            step.SetOrder(index++);
        }
    }
}

public sealed class DotnetRoadmapStep
{
    private DotnetRoadmapStep(string key, string name, int order)
    {
        Key = key.Trim();
        Name = name.Trim();
        Order = order;
    }

    public string Key { get; }

    public string Name { get; private set; }

    public int Order { get; private set; }

    public static DotnetRoadmapStep Create(
        string key,
        string name,
        int order)
    {
        return new DotnetRoadmapStep(key, name, order);
    }

    public void Rename(string name)
    {
        Name = name.Trim();
    }

    public void SetOrder(int order)
    {
        Order = order;
    }
}
