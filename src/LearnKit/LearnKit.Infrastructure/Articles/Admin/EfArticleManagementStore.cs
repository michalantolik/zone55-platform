using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Application.Articles.Public.Models;
using LearnKit.Domain.Articles;
using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.Entities;
using LearnKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Articles;

/// <summary>
/// Entity Framework implementation of
/// <see cref="IArticleManagementStore"/>.
/// </summary>
internal sealed class EfArticleManagementStore
    : IArticleManagementStore
{
    private readonly LearnKitDbContext _dbContext;

    /// <summary>
    /// Creates a new article management store.
    /// </summary>
    public EfArticleManagementStore(
        LearnKitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ArticleManagementListItem>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Articles
            .AsNoTracking()
            .OrderBy(article => article.LearningStepId)
            .ThenBy(article => article.SortOrder)
            .Select(article => new ArticleManagementListItem(
                article.Id,
                article.LearningStepId,
                article.Slug,
                article.Title,
                article.Summary,
                article.SortOrder,
                article.Status.ToString(),
                article.Blocks.Count))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<ArticleManagementDetails?> GetByIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(
            articleId,
            SupportedArticleLanguages.Default,
            cancellationToken);
    }

    public async Task<ArticleManagementDetails?> GetByIdAsync(
        Guid articleId,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedLanguage =
            SupportedArticleLanguages.Normalize(languageCode);

        var article = await _dbContext.Articles
            .AsNoTracking()
            .Include(item => item.Translations)
            .Include(item => item.Blocks)
                .ThenInclude(block => block.Translations)
            .Where(item => item.Id == articleId)
            .FirstOrDefaultAsync(cancellationToken);

        return article is null
            ? null
            : MapForEditing(article, normalizedLanguage);
    }

    /// <inheritdoc />
    public Task<Article?> GetTrackedByIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Articles
            .Include(article => article.Translations)
            .Include(article => article.Blocks)
                .ThenInclude(block => block.Translations)
            .FirstOrDefaultAsync(
                article => article.Id == articleId,
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludingArticleId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim();

        return _dbContext.Articles.AnyAsync(
            article =>
                article.Slug == normalizedSlug
                && (!excludingArticleId.HasValue
                    || article.Id != excludingArticleId.Value),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(
        Article article,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Articles.AddAsync(
            article,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Article>>
        GetTrackedByStepIdAsync(
            Guid learningStepId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Articles
            .Where(article =>
                article.LearningStepId == learningStepId)
            .ToListAsync(cancellationToken);
    }

    public void Remove(Article article)
    {
        _dbContext.Articles.Remove(article);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static ArticleManagementDetails MapForEditing(
        Article article,
        string requestedLanguage)
    {
        var requestedTranslation = article.GetTranslation(requestedLanguage);
        var hasCompleteTranslation =
            requestedTranslation is not null
            && article.Blocks.All(
                block => block.GetTranslation(requestedLanguage) is not null);

        var contentLanguage = requestedTranslation is not null
            ? requestedLanguage
            : SupportedArticleLanguages.Default;

        var selectedTranslation = article.GetTranslation(contentLanguage);

        var blocks = article.Blocks
            .Select(block => MapBlock(
                block,
                contentLanguage,
                requestedLanguage))
            .ToList();

        return new ArticleManagementDetails(
            article.Id,
            article.LearningStepId,
            article.Slug,
            selectedTranslation?.Title ?? article.Title,
            selectedTranslation?.Summary ?? article.Summary,
            article.SortOrder,
            selectedTranslation?.Status.ToString() ?? article.Status.ToString(),
            blocks,
            requestedLanguage,
            requestedTranslation is not null,
            !hasCompleteTranslation,
            article.Translations
                .Select(translation => translation.LanguageCode)
                .OrderBy(language => language)
                .ToList());
    }

    private static ArticleBlockDetails MapBlock(
        ArticleBlock block,
        string contentLanguage,
        string requestedLanguage)
    {
        return new ArticleBlockDetails(
            block.Id,
            block.Type.ToString(),
            block.SortOrder,
            block.GetTranslation(requestedLanguage)?.ContentJson
                ?? block.GetTranslation(contentLanguage)?.ContentJson
                ?? block.GetTranslation(SupportedArticleLanguages.Default)?.ContentJson
                ?? block.ContentJson);
    }
}
