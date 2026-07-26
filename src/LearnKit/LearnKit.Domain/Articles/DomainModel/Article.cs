using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.DomainModel;
using LearnKit.Domain.Articles.Entities;

namespace LearnKit.Domain.Articles;

/// <summary>
/// Represents one article in a learning step.
///
/// An article owns its blocks and language versions.
/// </summary>
public sealed class Article
{
    private readonly List<ArticleBlock> _blocks = [];
    private readonly List<ArticleTranslation> _translations = [];

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
        string? summary = null,
        string languageCode = SupportedArticleLanguages.Default)
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

        _translations.Add(
            new ArticleTranslation(
                languageCode,
                title,
                summary));
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
    /// URL-friendly article identifier shared by every language.
    /// </summary>
    public string Slug { get; private set; }

    /// <summary>
    /// Legacy article title retained during the localization migration.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Position of the article inside its learning step.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Legacy article summary retained during the localization migration.
    /// </summary>
    public string Summary { get; private set; }

    /// <summary>
    /// Legacy publishing state retained during the localization migration.
    /// </summary>
    public ArticleStatus Status { get; private set; }

    /// <summary>
    /// Language-specific versions of the article.
    /// </summary>
    public IReadOnlyCollection<ArticleTranslation> Translations =>
        _translations.ToList();

    /// <summary>
    /// Ordered article blocks.
    /// </summary>
    public IReadOnlyCollection<ArticleBlock> Blocks =>
        _blocks.OrderBy(block => block.SortOrder).ToList();

    /// <summary>
    /// Indicates whether the legacy article version is published.
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
    /// Changes the slug shared by every language version.
    /// </summary>
    public void ChangeSlug(string slug)
    {
        ValidateRequired(slug, nameof(slug), "Article slug is required.");

        Slug = slug.Trim();
    }

    /// <summary>
    /// Renames the legacy and default language versions.
    /// </summary>
    public void Rename(string title)
    {
        ValidateRequired(title, nameof(title), "Article title is required.");

        Title = title.Trim();

        SetTranslation(
            SupportedArticleLanguages.Default,
            Title,
            Summary);
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
    /// Updates the legacy and default language summaries.
    /// </summary>
    public void UpdateSummary(string? summary)
    {
        Summary = NormalizeOptional(summary);

        SetTranslation(
            SupportedArticleLanguages.Default,
            Title,
            Summary);
    }

    /// <summary>
    /// Creates or updates an article language version.
    /// </summary>
    public ArticleTranslation SetTranslation(
        string languageCode,
        string title,
        string? summary)
    {
        var normalizedLanguage =
            SupportedArticleLanguages.Normalize(languageCode);

        var translation = _translations.SingleOrDefault(
            item => item.LanguageCode == normalizedLanguage);

        if (translation is null)
        {
            translation = new ArticleTranslation(
                normalizedLanguage,
                title,
                summary);

            _translations.Add(translation);
        }
        else
        {
            translation.Update(title, summary);
        }

        return translation;
    }

    /// <summary>
    /// Returns the selected article language version.
    /// </summary>
    public ArticleTranslation? GetTranslation(string languageCode)
    {
        var normalizedLanguage =
            SupportedArticleLanguages.Normalize(languageCode);

        return _translations.SingleOrDefault(
            translation => translation.LanguageCode == normalizedLanguage);
    }

    /// <summary>
    /// Publishes the legacy and default language versions.
    /// </summary>
    public void Publish()
    {
        Status = ArticleStatus.Published;

        GetTranslation(SupportedArticleLanguages.Default)?.Publish();
    }

    /// <summary>
    /// Publishes the selected language version.
    /// </summary>
    public bool PublishTranslation(string languageCode)
    {
        var translation = GetTranslation(languageCode);

        if (translation is null)
        {
            return false;
        }

        translation.Publish();

        return true;
    }

    /// <summary>
    /// Moves the legacy and default language versions back to draft.
    /// </summary>
    public void MoveToDraft()
    {
        Status = ArticleStatus.Draft;

        GetTranslation(SupportedArticleLanguages.Default)?.MoveToDraft();
    }

    /// <summary>
    /// Moves the selected language version back to draft.
    /// </summary>
    public bool MoveTranslationToDraft(string languageCode)
    {
        var translation = GetTranslation(languageCode);

        if (translation is null)
        {
            return false;
        }

        translation.MoveToDraft();

        return true;
    }

    public bool ArchiveTranslation(string languageCode)
    {
        var translation = GetTranslation(languageCode);

        if (translation is null)
        {
            return false;
        }

        translation.Archive();

        return true;
    }

    /// <summary>
    /// Archives the legacy and default language versions.
    /// </summary>
    public void Archive()
    {
        Status = ArticleStatus.Archived;

        GetTranslation(SupportedArticleLanguages.Default)?.Archive();
    }

    /// <summary>
    /// Adds a new block to the article.
    /// </summary>
    public void AddBlock(ArticleBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        foreach (var existingBlock in _blocks
                     .Where(existingBlock => existingBlock.SortOrder >= block.SortOrder))
        {
            existingBlock.MoveTo(existingBlock.SortOrder + 1);
        }

        _blocks.Add(block);
        NormalizeBlockOrder();
    }

    /// <summary>
    /// Updates an existing legacy block.
    /// </summary>
    public bool UpdateBlock(
        Guid blockId,
        ArticleBlockType type,
        string contentJson,
        string languageCode = SupportedArticleLanguages.Default)
    {
        var block = _blocks.FirstOrDefault(
            block => block.Id == blockId);

        if (block is null)
        {
            return false;
        }

        var normalizedLanguage =
            SupportedArticleLanguages.Normalize(languageCode);

        if (string.Equals(
                normalizedLanguage,
                SupportedArticleLanguages.Default,
                StringComparison.Ordinal))
        {
            block.Update(type, contentJson);
        }
        else
        {
            block.ChangeType(type);
        }

        block.SetTranslation(normalizedLanguage, contentJson);

        return true;
    }

    /// <summary>
    /// Creates or updates a translated version of an existing block.
    /// </summary>
    public bool SetBlockTranslation(
        Guid blockId,
        string languageCode,
        string contentJson)
    {
        var block = _blocks.FirstOrDefault(
            block => block.Id == blockId);

        if (block is null)
        {
            return false;
        }

        block.SetTranslation(languageCode, contentJson);

        return true;
    }

    /// <summary>
    /// Removes an existing block and all its translations.
    /// </summary>
    public bool RemoveBlock(Guid blockId)
    {
        var block = _blocks.FirstOrDefault(
            block => block.Id == blockId);

        if (block is null)
        {
            return false;
        }

        _blocks.Remove(block);
        NormalizeBlockOrder();

        return true;
    }

    /// <summary>
    /// Applies a complete order to the article blocks.
    /// </summary>
    public void ReorderBlocks(
        IReadOnlyCollection<Guid> orderedBlockIds)
    {
        ArgumentNullException.ThrowIfNull(orderedBlockIds);

        if (orderedBlockIds.Count != _blocks.Count
            || orderedBlockIds.Distinct().Count() != orderedBlockIds.Count
            || orderedBlockIds.Any(
                blockId => _blocks.All(block => block.Id != blockId)))
        {
            throw new ArgumentException(
                "Block order must contain every article block exactly once.",
                nameof(orderedBlockIds));
        }

        var sortOrder = 1;

        foreach (var blockId in orderedBlockIds)
        {
            var block = _blocks.Single(
                block => block.Id == blockId);

            block.MoveTo(sortOrder++);
        }
    }

    private void NormalizeBlockOrder()
    {
        var sortOrder = 1;

        foreach (var block in _blocks.OrderBy(
                     block => block.SortOrder))
        {
            block.MoveTo(sortOrder++);
        }
    }

    private static void ValidateId(
        Guid id,
        string parameterName)
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
            throw new ArgumentException(
                message,
                parameterName);
        }
    }

    private static string NormalizeOptional(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
