namespace LearnKit.Domain.Roadmaps;

/// <summary>
/// Represents one complete learning path.
///
/// A learning path groups ordered learning zones.
/// </summary>
public sealed class LearningPath
{
    private readonly List<LearningZone> _zones = [];

    /// <summary>
    /// Creates a new learning path.
    /// </summary>
    public LearningPath(
        string key,
        string title,
        string? summary)
    {
        ValidateRequired(key, nameof(key), "Learning path key is required.");
        ValidateRequired(title, nameof(title), "Learning path title is required.");

        Key = key.Trim();
        Title = title.Trim();
        Summary = NormalizeOptional(summary);
    }

    /// <summary>
    /// Unique article identifier.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Stable key used to identify the learning path.
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// Learning path title shown to the user.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Short learning path description.
    /// </summary>
    public string Summary { get; private set; }

    /// <summary>
    /// Ordered zones inside the learning path.
    /// </summary>
    public IReadOnlyCollection<LearningZone> Zones =>
        _zones.OrderBy(zone => zone.SortOrder).ToList();

    /// <summary>
    /// Renames the learning path.
    /// </summary>
    public void Rename(string title)
    {
        ValidateRequired(title, nameof(title), "Learning path title is required.");

        Title = title.Trim();
    }

    /// <summary>
    /// Updates the learning path summary.
    /// </summary>
    public void UpdateSummary(string? summary)
    {
        Summary = NormalizeOptional(summary);
    }

    /// <summary>
    /// Adds a zone to the learning path.
    /// </summary>
    public void AddZone(LearningZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);

        _zones.Add(zone);
        NormalizeZoneOrder();
    }

    public bool RemoveZone(Guid zoneId)
    {
        var zone = _zones.SingleOrDefault(candidate => candidate.Id == zoneId);

        if (zone is null)
        {
            return false;
        }

        if (zone.Steps.Count > 0)
        {
            throw new InvalidOperationException("A zone containing learning steps cannot be removed.");
        }

        _zones.Remove(zone);
        NormalizeZoneOrder();
        return true;
    }

    public void ReorderZones(IReadOnlyCollection<Guid> orderedZoneIds)
    {
        ArgumentNullException.ThrowIfNull(orderedZoneIds);
        ValidateCompleteOrder(_zones.Select(zone => zone.Id), orderedZoneIds, "zones");

        var zonesById = _zones.ToDictionary(zone => zone.Id);
        var sortOrder = 1;

        foreach (var zoneId in orderedZoneIds)
        {
            zonesById[zoneId].MoveTo(sortOrder++);
        }
    }

    private void NormalizeZoneOrder()
    {
        var sortOrder = 1;

        foreach (var zone in _zones.OrderBy(zone => zone.SortOrder))
        {
            zone.MoveTo(sortOrder++);
        }
    }

    private static void ValidateCompleteOrder(
        IEnumerable<Guid> currentIds,
        IReadOnlyCollection<Guid> orderedIds,
        string itemName)
    {
        var current = currentIds.ToHashSet();
        var proposed = orderedIds.ToHashSet();

        if (orderedIds.Count != proposed.Count || !current.SetEquals(proposed))
        {
            throw new ArgumentException($"The order must contain every {itemName} identifier exactly once.", nameof(orderedIds));
        }
    }

    private static void ValidateRequired(
        string? value,
        string parameterName,
        string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, parameterName);
        }
    }

    private static string NormalizeOptional(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
