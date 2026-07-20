namespace LearnKit.Domain.Articles;

/// <summary>
/// Represents one article in a learning step.
///
/// An article owns its blocks and controls its publishing state.
/// </summary>
public sealed class Article
{
    private readonly List<ArticleBlock> _blocks = [];

    /// <summary>
    /// Required by Entity Framework to materialize articles from the database.
    /// </summary>
    private Article()
    {
        Slug = string.Empty;
        Title = string.Empty;
        Summary = string.Empty;
    }

    /// <summary>
    /// Creates a new draft article assigned to a learning step.
    /// </summary>
    public Article(
        Guid learningStepId,
        string slug,
        string title,
        int sortOrder,
        string? summary = null)
    {
        ValidateId(learningStepId, nameof(learningStepId));
        ValidateRequired(slug, nameof(slug), "Article slug is required.");
        ValidateRequired(title, nameof(title), "Article title is required.");

        if (sortOrder < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Sort order must be greater than zero.");
        }

        LearningStepId = learningStepId;
        Slug = slug.Trim();
        Title = title.Trim();
        SortOrder = sortOrder;
        Summary = NormalizeOptional(summary);
        Status = ArticleStatus.Draft;
    }

    /// <summary>
    /// Unique article identifier.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Learning step that owns this article.
    /// </summary>
    public Guid LearningStepId { get; private set; }

    /// <summary>
    /// URL-friendly article identifier.
    /// </summary>
    public string Slug { get; private set; }

    /// <summary>
    /// Article title shown to the user.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Position of the article inside its learning step.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Short article description.
    /// </summary>
    public string Summary { get; private set; }

    /// <summary>
    /// Current article publishing state.
    /// </summary>
    public ArticleStatus Status { get; private set; }

    /// <summary>
    /// Ordered article blocks.
    /// </summary>
    public IReadOnlyCollection<ArticleBlock> Blocks =>
        _blocks.OrderBy(block => block.SortOrder).ToList();

    /// <summary>
    /// Indicates whether the article is published.
    /// </summary>
    public bool IsPublished => Status == ArticleStatus.Published;

    /// <summary>
    /// Moves the article to another learning step.
    /// </summary>
    public void MoveToStep(Guid learningStepId)
    {
        ValidateId(learningStepId, nameof(learningStepId));

        LearningStepId = learningStepId;
    }

    /// <summary>
    /// Changes the article slug.
    /// </summary>
    public void ChangeSlug(string slug)
    {
        ValidateRequired(slug, nameof(slug), "Article slug is required.");

        Slug = slug.Trim();
    }

    /// <summary>
    /// Renames the article.
    /// </summary>
    public void Rename(string title)
    {
        ValidateRequired(title, nameof(title), "Article title is required.");

        Title = title.Trim();
    }

    /// <summary>
    /// Changes the article position inside its learning step.
    /// </summary>
    public void ChangeSortOrder(int sortOrder)
    {
        if (sortOrder < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Sort order must be greater than zero.");
        }

        SortOrder = sortOrder;
    }

    /// <summary>
    /// Updates the article summary.
    /// </summary>
    public void UpdateSummary(string? summary)
    {
        Summary = NormalizeOptional(summary);
    }

    /// <summary>
    /// Publishes the article.
    /// </summary>
    public void Publish()
    {
        Status = ArticleStatus.Published;
    }

    /// <summary>
    /// Moves the article back to draft.
    /// </summary>
    public void MoveToDraft()
    {
        Status = ArticleStatus.Draft;
    }

    /// <summary>
    /// Archives the article.
    /// </summary>
    public void Archive()
    {
        Status = ArticleStatus.Archived;
    }

    /// <summary>
    /// Adds a new block to the article.
    /// </summary>
    public void AddBlock(ArticleBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        _blocks.Add(block);
        ReorderBlocks();
    }

    private void ReorderBlocks()
    {
        var sortOrder = 1;

        foreach (var block in _blocks.OrderBy(block => block.SortOrder))
        {
            block.MoveTo(sortOrder++);
        }
    }

    private static void ValidateId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Identifier cannot be empty.",
                parameterName);
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
