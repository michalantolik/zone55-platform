using LearnKit.Domain.Articles;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for LearnKit.
/// </summary>
public sealed class LearnKitDbContext : DbContext
{
    public LearnKitDbContext(DbContextOptions<LearnKitDbContext> options) : base(options) { }

    public DbSet<Article> Articles => Set<Article>();

    public DbSet<ArticleBlock> ArticleBlocks => Set<ArticleBlock>();

    /// <summary>
    /// Applies entity configurations from this assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(LearnKitDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
