using LearnKit.Domain.Articles;

namespace LearnKit.Domain.Roadmaps;

/// <summary>
/// Represents one ordered step inside a learning zone.
/// </summary>
public sealed class LearningStep
{
    private readonly List<Article> _articles = [];

    /// <summary>
    /// Creates a new learning step.
    /// </summary>
    public LearningStep(
        string key,
        string title,
        string? summary,
        int sortOrder)
    {
        ValidateRequired(key, nameof(key), "Step key is required.");
        ValidateRequired(title, nameof(title), "Step title is required.");
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
    /// Stable key used to identify the step.
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// Step title shown to the user.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Short step description.
    /// </summary>
    public string Summary { get; private set; }

    /// <summary>
    /// Determines the position inside the zone.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Ordered articles inside the step.
    /// </summary>
    public IReadOnlyCollection<Article> Articles =>
        _articles.ToList();

    /// <summary>
    /// Renames the step.
    /// </summary>
    public void Rename(string title)
    {
        ValidateRequired(title, nameof(title), "Step title is required.");

        Title = title.Trim();
    }

    /// <summary>
    /// Updates the step summary.
    /// </summary>
    public void UpdateSummary(string? summary)
    {
        Summary = NormalizeOptional(summary);
    }

    /// <summary>
    /// Moves the step to a new position.
    /// </summary>
    public void MoveTo(int sortOrder)
    {
        ValidateSortOrder(sortOrder);

        SortOrder = sortOrder;
    }

    /// <summary>
    /// Adds an article to the step.
    /// </summary>
    public void AddArticle(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        article.MoveToStep(Id);

        _articles.Add(article);
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