using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Application.Articles.Public.Models;
using LearnKit.Domain.Articles;
using LearnKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Articles;

/// <summary>
/// Entity Framework implementation of <see cref="IArticleManagementStore"/>.
/// </summary>
internal sealed class EfArticleManagementStore : IArticleManagementStore
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
    public async Task<IReadOnlyCollection<ArticleManagementListItem>> GetAllAsync(
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
    public async Task<ArticleManagementDetails?> GetByIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Articles
            .AsNoTracking()
            .Where(article => article.Id == articleId)
            .Select(article => new ArticleManagementDetails(
                article.Id,
                article.LearningStepId,
                article.Slug,
                article.Title,
                article.Summary,
                article.SortOrder,
                article.Status.ToString(),
                article.Blocks
                    .OrderBy(block => block.SortOrder)
                    .Select(block => new ArticleBlockDetails(
                        block.Id,
                        block.Type.ToString(),
                        block.SortOrder,
                        block.ContentJson))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Article?> GetTrackedByIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Articles
            .Include(article => article.Blocks)
            .FirstOrDefaultAsync(
                article => article.Id == articleId,
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

    public async Task<IReadOnlyCollection<Article>> GetTrackedByStepIdAsync(
        Guid learningStepId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Articles
            .Where(article => article.LearningStepId == learningStepId)
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
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
