using LearnKit.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKit.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the <see cref="LearningStep"/> entity.
/// </summary>
internal sealed class LearningStepConfiguration : IEntityTypeConfiguration<LearningStep>
{
    public void Configure(EntityTypeBuilder<LearningStep> learningStep)
    {
        learningStep.ToTable("LearningSteps");

        learningStep.HasKey(x => x.Id);

        learningStep.Property(x => x.Id)
            .ValueGeneratedNever();

        learningStep.HasIndex(x => x.Key)
            .IsUnique();

        learningStep.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        learningStep.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        learningStep.Property(x => x.Summary)
            .HasMaxLength(2000);

        learningStep.Property(x => x.SortOrder)
            .IsRequired();
    }
}
