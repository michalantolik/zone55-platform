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
        ReorderZones();
    }

    private void ReorderZones()
    {
        var sortOrder = 1;

        foreach (var zone in _zones.OrderBy(zone => zone.SortOrder))
        {
            zone.MoveTo(sortOrder++);
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
