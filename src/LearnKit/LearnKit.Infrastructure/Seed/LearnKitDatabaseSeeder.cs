using LearnKit.Domain.Articles;
using LearnKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Seed;

/// <summary>
/// Seeds initial LearnKit data for local development.
/// </summary>
public sealed class LearnKitDatabaseSeeder
{
    private readonly LearnKitDbContext _dbContext;

    public LearnKitDatabaseSeeder(LearnKitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var hasArticles = await _dbContext.Articles
            .AnyAsync(cancellationToken);

        if (hasArticles)
        {
            return;
        }
    }
}
