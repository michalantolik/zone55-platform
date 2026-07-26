using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.DomainModel;

namespace LearnKit.Domain.Articles.Entities;

/// <summary>
/// Represents one language version of an article.
/// </summary>
public sealed class ArticleTranslation
{
    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private ArticleTranslation()
    {
        LanguageCode = string.Empty;
        Title = string.Empty;
        Summary = string.Empty;
    }

    /// <summary>
    /// Creates a new draft translation of an article.
    /// </summary>
    public ArticleTranslation(
        string languageCode,
        string title,
        string? summary = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        LanguageCode = SupportedArticleLanguages.Normalize(languageCode);
        Title = title.Trim();
        Summary = NormalizeOptional(summary);
        Status = ArticleStatus.Draft;
    }

    /// <summary>
    /// Unique translation identifier.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Language represented by this translation.
    /// </summary>
    public string LanguageCode { get; private set; }

    /// <summary>
    /// Translated article title.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Translated article summary.
    /// </summary>
    public string Summary { get; private set; }

    /// <summary>
    /// Publishing state of this language version.
    /// </summary>
    public ArticleStatus Status { get; private set; }

    /// <summary>
    /// Updates the translated article title and summary.
    /// </summary>
    public void Update(string title, string? summary)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title.Trim();
        Summary = NormalizeOptional(summary);
    }

    /// <summary>
    /// Publishes this language version.
    /// </summary>
    public void Publish()
    {
        Status = ArticleStatus.Published;
    }

    /// <summary>
    /// Moves this language version back to draft.
    /// </summary>
    public void MoveToDraft()
    {
        Status = ArticleStatus.Draft;
    }

    /// <summary>
    /// Archives this language version.
    /// </summary>
    public void Archive()
    {
        Status = ArticleStatus.Archived;
    }

    private static string NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }
}
