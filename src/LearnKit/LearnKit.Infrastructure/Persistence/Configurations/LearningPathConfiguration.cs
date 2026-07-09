using LearnKit.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKit.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the <see cref="LearningPath"/> entity.
/// </summary>
internal sealed class LearningPathConfiguration : IEntityTypeConfiguration<LearningPath>
{
    public void Configure(EntityTypeBuilder<LearningPath> learningPath)
    {
        learningPath.ToTable("LearningPaths");

        learningPath.HasKey(x => x.Id);

        learningPath.Property(x => x.Id)
            .ValueGeneratedNever();

        learningPath.HasIndex(x => x.Key)
            .IsUnique();

        learningPath.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        learningPath.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        learningPath.Property(x => x.Summary)
            .HasMaxLength(2000);

        learningPath.HasMany(x => x.Zones)
            .WithOne()
            .HasForeignKey("LearningPathId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
