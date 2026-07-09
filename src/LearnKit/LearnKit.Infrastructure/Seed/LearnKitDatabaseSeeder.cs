using LearnKit.Domain.Articles;
using LearnKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Seed;

/// <summary>
/// Seeds initial LearnKit data for local development.
/// </summary>
public sealed class LearnKitDatabaseSeeder
{}
    public LearnKitDatabaseSeeder(LearnKitDbContext dbContext)
    {

    }
}
