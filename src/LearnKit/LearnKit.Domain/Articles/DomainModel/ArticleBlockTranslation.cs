using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.DomainModel;

namespace LearnKit.Domain.Articles.Entities;

/// <summary>
/// Represents language-specific content of an article block.
/// </summary>
public sealed class ArticleBlockTranslation
{
    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private ArticleBlockTranslation()
    {
        LanguageCode = string.Empty;
        ContentJson = string.Empty;
    }

    /// <summary>
    /// Creates a language version of an article block.
    /// </summary>
    public ArticleBlockTranslation(
        string languageCode,
        ArticleBlockType blockType,
        string contentJson)
    {
        var normalizedLanguage =
            SupportedArticleLanguages.Normalize(languageCode);

        ArticleBlockContentValidator.Validate(
            blockType,
            contentJson);

        LanguageCode = normalizedLanguage;
        ContentJson = NormalizeContent(contentJson);
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
    /// Language-specific block content stored as JSON.
    /// </summary>
    public string ContentJson { get; private set; }

    /// <summary>
    /// Updates the translated block content.
    /// </summary>
    public void Update(
        ArticleBlockType blockType,
        string contentJson)
    {
        ArticleBlockContentValidator.Validate(
            blockType,
            contentJson);

        ContentJson = NormalizeContent(contentJson);
    }

    private static string NormalizeContent(string contentJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentJson);

        return contentJson.Trim();
    }
}
