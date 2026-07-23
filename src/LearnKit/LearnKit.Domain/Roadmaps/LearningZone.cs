namespace LearnKit.Domain.Roadmaps;

/// <summary>
/// Represents a major area inside a learning path.
///
/// A zone groups ordered learning steps.
/// </summary>
public sealed class LearningZone
{
    private readonly List<LearningStep> _steps = [];

    /// <summary>
    /// Creates a new learning zone.
    /// </summary>
    public LearningZone(
        string key,
        string title,
        string? summary,
        int sortOrder)
    {
        ValidateRequired(key, nameof(key), "Zone key is required.");
        ValidateRequired(title, nameof(title), "Zone title is required.");
        ValidateSortOrder(sortOrder);

        Key = key.Trim();
        Title = title.Trim();
        Summary = NormalizeOptional(summary);
        SortOrder = sortOrder;
    }

    /// <summary>
    /// Unique article identifier.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Stable key used to identify the zone.
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// Zone title shown to the user.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Short zone description.
    /// </summary>
    public string Summary { get; private set; }

    /// <summary>
    /// Determines the position inside the learning path.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Ordered steps inside the zone.
    /// </summary>
    public IReadOnlyCollection<LearningStep> Steps =>
        _steps.OrderBy(step => step.SortOrder).ToList();

    /// <summary>
    /// Renames the zone.
    /// </summary>
    public void Rename(string title)
    {
        ValidateRequired(title, nameof(title), "Zone title is required.");

        Title = title.Trim();
    }

    /// <summary>
    /// Updates the zone summary.
    /// </summary>
    public void UpdateSummary(string? summary)
    {
        Summary = NormalizeOptional(summary);
    }

    /// <summary>
    /// Moves the zone to a new position.
    /// </summary>
    public void MoveTo(int sortOrder)
    {
        ValidateSortOrder(sortOrder);

        SortOrder = sortOrder;
    }

    /// <summary>
    /// Adds a step to the zone.
    /// </summary>
    public void AddStep(LearningStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        _steps.Add(step);
        NormalizeStepOrder();
    }

    public bool RemoveStep(Guid stepId)
    {
        var step = _steps.SingleOrDefault(candidate => candidate.Id == stepId);

        if (step is null)
        {
            return false;
        }

        if (step.Articles.Count > 0)
        {
            throw new InvalidOperationException("A learning step containing articles cannot be removed.");
        }

        _steps.Remove(step);
        NormalizeStepOrder();
        return true;
    }

    public void ReorderSteps(IReadOnlyCollection<Guid> orderedStepIds)
    {
        ArgumentNullException.ThrowIfNull(orderedStepIds);

        var current = _steps.Select(step => step.Id).ToHashSet();
        var proposed = orderedStepIds.ToHashSet();

        if (orderedStepIds.Count != proposed.Count || !current.SetEquals(proposed))
        {
            throw new ArgumentException("The order must contain every step identifier exactly once.", nameof(orderedStepIds));
        }

        var stepsById = _steps.ToDictionary(step => step.Id);
        var sortOrder = 1;

        foreach (var stepId in orderedStepIds)
        {
            stepsById[stepId].MoveTo(sortOrder++);
        }
    }

    private void NormalizeStepOrder()
    {
        var sortOrder = 1;

        foreach (var step in _steps.OrderBy(step => step.SortOrder))
        {
            step.MoveTo(sortOrder++);
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

    private static void ValidateSortOrder(int sortOrder)
    {
        if (sortOrder < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Sort order must be greater than zero.");
        }
    }

    private static string NormalizeOptional(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
