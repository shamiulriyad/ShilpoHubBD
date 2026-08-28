using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Data.Configurations;

public class HeritageDatasetExportConfiguration : IEntityTypeConfiguration<HeritageDatasetExport>
{
    public void Configure(EntityTypeBuilder<HeritageDatasetExport> builder)
    {
        builder.ToTable("HeritageDatasetExports");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Format).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.FilterJson).HasMaxLength(4000);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.FileUrl).HasMaxLength(2048);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.HeritageDatasetId);
        builder.HasIndex(e => e.RequestedByUserId);
        builder.HasIndex(e => new { e.HeritageDatasetId, e.CreatedAt });

        builder.HasOne(e => e.RequestedBy)
            .WithMany()
            .HasForeignKey(e => e.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Version)
            .WithMany()
            .HasForeignKey(e => e.DatasetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
