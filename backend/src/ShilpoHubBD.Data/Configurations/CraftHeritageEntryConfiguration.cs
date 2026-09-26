using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Configurations;

public class CraftHeritageEntryConfiguration : IEntityTypeConfiguration<CraftHeritageEntry>
{
    public void Configure(EntityTypeBuilder<CraftHeritageEntry> builder)
    {
        builder.ToTable("CraftHeritageEntries");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Slug).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Aliases).HasMaxLength(500);
        builder.Property(e => e.Region).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Type).IsRequired().HasMaxLength(100);
        builder.Property(e => e.GiName).HasMaxLength(300);
        builder.Property(e => e.Unesco).HasMaxLength(300);
        builder.Property(e => e.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.History).HasMaxLength(4000);
        builder.Property(e => e.Materials).HasMaxLength(2000);
        builder.Property(e => e.Process).HasMaxLength(4000);
        builder.Property(e => e.Products).HasMaxLength(2000);
        builder.Property(e => e.Story).HasMaxLength(4000);
        builder.Property(e => e.Visit).HasMaxLength(2000);
        builder.Property(e => e.Sources).HasMaxLength(4000);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.DisplayOrder);
    }
}
