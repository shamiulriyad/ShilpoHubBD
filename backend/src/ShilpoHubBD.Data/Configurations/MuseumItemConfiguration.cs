using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Data.Configurations;

public class MuseumItemConfiguration : IEntityTypeConfiguration<MuseumItem>
{
    public void Configure(EntityTypeBuilder<MuseumItem> builder)
    {
        builder.ToTable("MuseumItems");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(4000);
        builder.Property(m => m.Category).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Era).HasMaxLength(100);
        builder.Property(m => m.CoverImageUrl).IsRequired().HasMaxLength(1000);
        builder.Property(m => m.ModelUrl).HasMaxLength(1000);
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();

        builder.HasIndex(m => m.Category);
        builder.HasIndex(m => m.IsActive);
        builder.HasIndex(m => m.DistrictId);

        builder.HasOne(m => m.District)
            .WithMany()
            .HasForeignKey(m => m.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Product)
            .WithMany()
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(m => m.Media)
            .WithOne(md => md.MuseumItem)
            .HasForeignKey(md => md.MuseumItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
