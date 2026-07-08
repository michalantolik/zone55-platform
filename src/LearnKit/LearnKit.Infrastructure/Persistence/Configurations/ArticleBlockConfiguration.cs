using LearnKit.Domain.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKit.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures how article blocks are stored in the database.
/// </summary>
internal sealed class ArticleBlockConfiguration : IEntityTypeConfiguration<ArticleBlock>
{
    public void Configure(EntityTypeBuilder<ArticleBlock> block)
    {
        block.ToTable("ArticleBlocks");

        block.HasKey(x => x.Id);

        block.Property(x => x.Id)
            .ValueGeneratedNever();

        block.Property(x => x.Type)
            .HasMaxLength(100)
            .IsRequired();

        block.Property(x => x.SortOrder)
            .IsRequired();

        block.Property(x => x.ContentJson)
            .IsRequired();

        block.HasIndex("ArticleId", nameof(ArticleBlock.SortOrder));
    }
}
