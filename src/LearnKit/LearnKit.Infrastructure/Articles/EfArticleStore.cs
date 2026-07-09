using LearnKit.Application.Articles.Contracts;
using LearnKit.Domain.Articles;
using LearnKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Articles;

/// <summary>
/// Entity Framework implementation of <see cref="IArticleStore"/>.
/// </summary>
internal sealed class EfArticleStore : IArticleStore
{
    private readonly LearnKitDbContext _dbContext;

    public EfArticleStore(LearnKitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(
        Article article,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Articles.AddAsync(article, cancellationToken).AsTask();
    }

    public Task<Article?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Articles
            .Include(article => article.Blocks)
            .FirstOrDefaultAsync(
                article => article.Id == id,
                cancellationToken);
    }

    public Task<Article?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Articles
            .Include(article => article.Blocks)
            .FirstOrDefaultAsync(
                article => article.Slug == slug,
                cancellationToken);
    }

    public Task RemoveAsync(
        Article article,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Articles.Remove(article);

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Articles
            .AnyAsync(
                article => article.Slug == slug,
                cancellationToken);
    }
}
