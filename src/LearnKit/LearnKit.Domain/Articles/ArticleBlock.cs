namespace LearnKit.Domain.Articles;

/// <summary>
/// Represents a single block inside an article.
///
/// Each block has a type, order, and JSON content.
/// </summary>
public sealed class ArticleBlock
{
    /// <summary>
    /// Creates a new article block.
    /// </summary>
    public ArticleBlock(
        ArticleBlockType type,
        int sortOrder,
        string contentJson)
    {
        ValidateSortOrder(sortOrder);
        ValidateContent(contentJson);

        Type = type;
        SortOrder = sortOrder;
        ContentJson = NormalizeContent(contentJson);
    }

    /// <summary>
    /// Unique block identifier.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Determines how the block should be rendered.
    /// </summary>
    public ArticleBlockType Type { get; private set; }

    /// <summary>
    /// Determines the position inside the article.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Block content stored as JSON.
    /// </summary>
    public string ContentJson { get; private set; }

    /// <summary>
    /// Changes the block type.
    /// </summary>
    public void ChangeType(ArticleBlockType type)
    {
        Type = type;
    }

    /// <summary>
    /// Updates the block content.
    /// </summary>
    public void UpdateContent(string contentJson)
    {
        ValidateContent(contentJson);
        ContentJson = NormalizeContent(contentJson);
    }

    /// <summary>
    /// Moves the block to a new position.
    /// </summary>
    public void MoveTo(int sortOrder)
    {
        ValidateSortOrder(sortOrder);
        SortOrder = sortOrder;
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

    private static void ValidateContent(string contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
        {
            throw new ArgumentException(
                "Block content cannot be empty.",
                nameof(contentJson));
        }
    }

    private static string NormalizeContent(string contentJson)
    {
        return contentJson.Trim();
    }
}
