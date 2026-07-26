using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.DomainModel;

namespace LearnKit.Domain.Articles.Entities;

/// <summary>
/// Represents a single block inside an article.
///
/// Each block has a type, order, and language-specific content.
/// </summary>
public sealed class ArticleBlock
{
    private readonly List<ArticleBlockTranslation> _translations = [];

    private ArticleBlock()
    {
        ContentJson = string.Empty;
    }

    /// <summary>
    /// Creates a new article block.
    /// </summary>
    public ArticleBlock(
        ArticleBlockType type,
        int sortOrder,
        string contentJson)
        : this(
            type,
            sortOrder,
            contentJson,
            SupportedArticleLanguages.Default)
    {
    }

    public ArticleBlock(
        ArticleBlockType type,
        int sortOrder,
        string contentJson,
        string languageCode)
    {
        ValidateSortOrder(sortOrder);
        ArticleBlockContentValidator.Validate(type, contentJson);

        Type = type;
        SortOrder = sortOrder;
        ContentJson = NormalizeContent(contentJson);

        _translations.Add(
            new ArticleBlockTranslation(
                languageCode,
                type,
                contentJson));
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
    /// Legacy block content retained during the localization migration.
    /// </summary>
    public string ContentJson { get; private set; }

    /// <summary>
    /// Language-specific versions of the block content.
    /// </summary>
    public IReadOnlyCollection<ArticleBlockTranslation> Translations =>
        _translations.ToList();

    /// <summary>
    /// Changes the block type and its legacy content.
    /// </summary>
    public void Update(
        ArticleBlockType type,
        string contentJson)
    {
        ArticleBlockContentValidator.Validate(type, contentJson);

        foreach (var translation in _translations)
        {
            ArticleBlockContentValidator.Validate(
                type,
                translation.ContentJson);
        }

        Type = type;
        ContentJson = NormalizeContent(contentJson);
    }

    public void ChangeType(ArticleBlockType type)
    {
        ArticleBlockContentValidator.Validate(type, ContentJson);

        foreach (var translation in _translations)
        {
            ArticleBlockContentValidator.Validate(type, translation.ContentJson);
        }

        Type = type;
    }

    /// <summary>
    /// Creates or updates content for the selected language.
    /// </summary>
    public ArticleBlockTranslation SetTranslation(
        string languageCode,
        string contentJson)
    {
        var normalizedLanguage =
            SupportedArticleLanguages.Normalize(languageCode);

        var translation = _translations.SingleOrDefault(
            item => item.LanguageCode == normalizedLanguage);

        if (translation is null)
        {
            translation = new ArticleBlockTranslation(
                normalizedLanguage,
                Type,
                contentJson);

            _translations.Add(translation);
        }
        else
        {
            translation.Update(Type, contentJson);
        }

        return translation;
    }

    /// <summary>
    /// Returns content for the selected language.
    /// </summary>
    public ArticleBlockTranslation? GetTranslation(string languageCode)
    {
        var normalizedLanguage =
            SupportedArticleLanguages.Normalize(languageCode);

        return _translations.SingleOrDefault(
            translation => translation.LanguageCode == normalizedLanguage);
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

    private static string NormalizeContent(string contentJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentJson);

        return contentJson.Trim();
    }
}
