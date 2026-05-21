using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Persistence;

public sealed class BlogPlatformDbContext : DbContext
{
    public BlogPlatformDbContext(DbContextOptions<BlogPlatformDbContext> options)
        : base(options)
    {
    }

    public DbSet<RoadmapZoneEntity> RoadmapZones => Set<RoadmapZoneEntity>();

    public DbSet<RoadmapStepEntity> RoadmapSteps => Set<RoadmapStepEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoadmapZoneEntity>(entity =>
        {
            entity.ToTable("BlogZone", "blogPlatform");

            entity.HasKey(zone => zone.RoadmapZoneId);

            entity.Property(zone => zone.Key)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(zone => zone.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(zone => zone.Order)
                .IsRequired();

            entity.HasIndex(zone => zone.Key)
                .IsUnique();

            entity.HasMany(zone => zone.Steps)
                .WithOne(step => step.Zone)
                .HasForeignKey(step => step.RoadmapZoneId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoadmapStepEntity>(entity =>
        {
            entity.ToTable("BlogStep", "blogPlatform");

            entity.HasKey(step => step.RoadmapStepId);

            entity.Property(step => step.Key)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(step => step.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(step => step.Icon)
                .HasMaxLength(32)
                .HasDefaultValue("📘")
                .IsRequired();

            entity.Property(step => step.Order)
                .IsRequired();

            entity.HasIndex(step => step.Key)
                .IsUnique();
        });
    }
}
