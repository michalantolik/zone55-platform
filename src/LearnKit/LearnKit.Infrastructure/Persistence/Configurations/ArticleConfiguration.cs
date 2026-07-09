using LearnKit.Domain.Articles;
using LearnKit.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKit.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the <see cref="Article"/> entity.
/// </summary>
internal sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> article)
    {
        article.ToTable("Articles");

        article.HasKey(x => x.Id);

        article.Property(x => x.Id)
            .ValueGeneratedNever();

        article.HasIndex(x => x.Slug)
            .IsUnique();

        article.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(200);

        article.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        article.Property(x => x.Summary)
            .HasMaxLength(2000);

        article.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        article.HasOne<LearningStep>()
            .WithMany(step => step.Articles)
            .HasForeignKey(x => x.LearningStepId)
            .OnDelete(DeleteBehavior.Cascade);

        article.HasMany(x => x.Blocks)
            .WithOne()
            .HasForeignKey("ArticleId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
