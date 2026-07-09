using LearnKit.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKit.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the <see cref="LearningZone"/> entity.
/// </summary>
internal sealed class LearningZoneConfiguration : IEntityTypeConfiguration<LearningZone>
{
    public void Configure(EntityTypeBuilder<LearningZone> learningZone)
    {
        learningZone.ToTable("LearningZones");

        learningZone.HasKey(x => x.Id);

        learningZone.Property(x => x.Id)
            .ValueGeneratedNever();

        learningZone.HasIndex(x => x.Key)
            .IsUnique();

        learningZone.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        learningZone.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        learningZone.Property(x => x.Summary)
            .HasMaxLength(2000);

        learningZone.Property(x => x.SortOrder)
            .IsRequired();

        learningZone.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey("LearningZoneId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
