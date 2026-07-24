using LearnKit.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKit.Infrastructure.Persistence.Configurations;

internal sealed class LearnKitInitializationRecordConfiguration
    : IEntityTypeConfiguration<LearnKitInitializationRecord>
{
    public void Configure(EntityTypeBuilder<LearnKitInitializationRecord> builder)
    {
        builder.ToTable("LearnKitInitializations");

        builder.HasKey(record => record.Key);

        builder.Property(record => record.Key)
            .HasMaxLength(100);

        builder.Property(record => record.SourceVersion)
            .HasMaxLength(100)
            .IsRequired();
    }
}
