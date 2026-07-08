using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for LearnKit.
/// </summary>
public sealed class LearnKitDbContext : DbContext
{
    public LearnKitDbContext(DbContextOptions<LearnKitDbContext> options) : base(options)
    {

    }
}
