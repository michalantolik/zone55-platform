using LearnKit.Domain.Articles;
using LearnKit.Domain.Articles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKit.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures language-specific article data.
/// </summary>
internal sealed class ArticleTranslationConfiguration
    : IEntityTypeConfiguration<ArticleTranslation>
{
    public void Configure(
        EntityTypeBuilder<ArticleTranslation> translation)
    {
        translation.ToTable("ArticleTranslations");

        translation.HasKey(x => x.Id);

        translation.Property(x => x.Id)
            .ValueGeneratedNever();

        translation.Property(x => x.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);

        translation.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        translation.Property(x => x.Summary)
            .HasMaxLength(2000);

        translation.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        translation.HasOne<Article>()
            .WithMany(article => article.Translations)
            .HasForeignKey("ArticleId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        translation.HasIndex(
                "ArticleId",
                nameof(ArticleTranslation.LanguageCode))
            .IsUnique();
    }
}
