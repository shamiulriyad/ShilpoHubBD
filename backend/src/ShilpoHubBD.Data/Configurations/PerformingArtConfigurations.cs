using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.PerformingArts;

namespace ShilpoHubBD.Data.Configurations;

public class PerformingArtRecordConfiguration : IEntityTypeConfiguration<PerformingArtRecord>
{
    public void Configure(EntityTypeBuilder<PerformingArtRecord> builder)
    {
        builder.ToTable("PerformingArtRecords");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Slug).IsRequired().HasMaxLength(120);
        builder.Property(r => r.TitleEn).IsRequired().HasMaxLength(200);
        builder.Property(r => r.TitleBn).HasMaxLength(200);
        builder.Property(r => r.AlternateNames).HasMaxLength(500);
        builder.Property(r => r.Category).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(r => r.SummaryEn).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.SummaryBn).HasMaxLength(2000);
        builder.Property(r => r.DescriptionEn).HasMaxLength(8000);
        builder.Property(r => r.DescriptionBn).HasMaxLength(8000);
        builder.Property(r => r.RegionalContextEn).HasMaxLength(4000);
        builder.Property(r => r.RegionalContextBn).HasMaxLength(4000);
        builder.Property(r => r.Division).HasMaxLength(40);
        builder.Property(r => r.Upazila).HasMaxLength(120);
        builder.Property(r => r.Communities).HasMaxLength(1000);
        builder.Property(r => r.Artists).HasMaxLength(4000);
        builder.Property(r => r.Instruments).HasMaxLength(1000);
        builder.Property(r => r.ViabilityNote).HasMaxLength(1000);
        builder.Property(r => r.InventoryReference).HasMaxLength(100);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.ReviewNote).HasMaxLength(1000);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasOne(r => r.District).WithMany().HasForeignKey(r => r.DistrictId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(r => r.Sources).WithOne(s => s.Record).HasForeignKey(s => s.RecordId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.Media).WithOne(m => m.Record).HasForeignKey(m => m.RecordId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Slug).IsUnique();
        builder.HasIndex(r => new { r.Status, r.Category });
        builder.HasIndex(r => new { r.Status, r.Division });
        builder.HasIndex(r => r.DistrictId);
        builder.HasIndex(r => new { r.Status, r.IsFeatured });
    }
}

public class PerformingArtSourceConfiguration : IEntityTypeConfiguration<PerformingArtSource>
{
    public void Configure(EntityTypeBuilder<PerformingArtSource> builder)
    {
        builder.ToTable("PerformingArtSources");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SourceType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(s => s.Title).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Publisher).HasMaxLength(300);
        builder.Property(s => s.Url).HasMaxLength(1000);
        builder.Property(s => s.Notes).HasMaxLength(1000);
        builder.HasIndex(s => new { s.RecordId, s.DisplayOrder });
    }
}

public class PerformingArtMediaConfiguration : IEntityTypeConfiguration<PerformingArtMedia>
{
    public void Configure(EntityTypeBuilder<PerformingArtMedia> builder)
    {
        builder.ToTable("PerformingArtMedia");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MediaType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.Url).IsRequired().HasMaxLength(1000);
        builder.Property(m => m.CaptionEn).HasMaxLength(500);
        builder.Property(m => m.CaptionBn).HasMaxLength(500);
        builder.Property(m => m.Credit).IsRequired().HasMaxLength(300);
        builder.Property(m => m.License).HasMaxLength(200);
        builder.Property(m => m.SourceUrl).HasMaxLength(1000);
        builder.HasIndex(m => new { m.RecordId, m.DisplayOrder });
    }
}
