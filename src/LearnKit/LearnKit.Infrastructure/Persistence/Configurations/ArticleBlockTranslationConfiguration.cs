using LearnKit.Domain.Articles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKit.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures language-specific article block content.
/// </summary>
internal sealed class ArticleBlockTranslationConfiguration
    : IEntityTypeConfiguration<ArticleBlockTranslation>
{
    public void Configure(
        EntityTypeBuilder<ArticleBlockTranslation> translation)
    {
        translation.ToTable("ArticleBlockTranslations");

        translation.HasKey(x => x.Id);

        translation.Property(x => x.Id)
            .ValueGeneratedNever();

        translation.Property(x => x.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);

        translation.Property(x => x.ContentJson)
            .IsRequired();

        translation.HasOne<ArticleBlock>()
            .WithMany(block => block.Translations)
            .HasForeignKey("ArticleBlockId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        translation.HasIndex(
                "ArticleBlockId",
                nameof(ArticleBlockTranslation.LanguageCode))
            .IsUnique();
    }
}
