using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Data.Configurations;

public class HeritageDatasetConfiguration : IEntityTypeConfiguration<HeritageDataset>
{
    public void Configure(EntityTypeBuilder<HeritageDataset> builder)
    {
        builder.ToTable("HeritageDatasets");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Slug).IsRequired().HasMaxLength(160);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Description).IsRequired().HasMaxLength(4000);
        builder.Property(d => d.Category).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.AccessLevel).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.SourceType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.SourceOrganization).HasMaxLength(200);
        builder.Property(d => d.SourceReference).HasMaxLength(500);
        builder.Property(d => d.License).HasMaxLength(200);
        builder.Property(d => d.Tags).HasMaxLength(500);
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt).IsRequired();

        builder.HasIndex(d => d.Slug).IsUnique();
        builder.HasIndex(d => d.OwnerUserId);
        builder.HasIndex(d => d.Category);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.AccessLevel);

        builder.HasOne(d => d.Owner)
            .WithMany()
            .HasForeignKey(d => d.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Versions)
            .WithOne(v => v.Dataset)
            .HasForeignKey(v => v.HeritageDatasetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.AccessGrants)
            .WithOne(g => g.Dataset)
            .HasForeignKey(g => g.HeritageDatasetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Exports)
            .WithOne(e => e.Dataset)
            .HasForeignKey(e => e.HeritageDatasetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.CurrentVersion)
            .WithMany()
            .HasForeignKey(d => d.CurrentVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
