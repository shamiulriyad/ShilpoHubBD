using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Data.Configurations;

public class HeritageDatasetVersionConfiguration : IEntityTypeConfiguration<HeritageDatasetVersion>
{
    public void Configure(EntityTypeBuilder<HeritageDatasetVersion> builder)
    {
        builder.ToTable("HeritageDatasetVersions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Label).HasMaxLength(50);
        builder.Property(v => v.Changelog).IsRequired().HasMaxLength(4000);
        builder.Property(v => v.Format).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(v => v.SourceFileName).HasMaxLength(260);
        builder.Property(v => v.SourceFileUrl).HasMaxLength(2048);
        builder.Property(v => v.SourceContentHash).HasMaxLength(128);
        builder.Property(v => v.ImportNotes).HasMaxLength(2000);
        builder.Property(v => v.SchemaJson).HasMaxLength(16000);
        builder.Property(v => v.CreatedAt).IsRequired();

        builder.HasIndex(v => new { v.HeritageDatasetId, v.VersionNumber }).IsUnique();

        builder.HasOne(v => v.CreatedBy)
            .WithMany()
            .HasForeignKey(v => v.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
