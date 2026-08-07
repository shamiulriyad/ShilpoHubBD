using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);

        builder.Property(p => p.Slug).IsRequired().HasMaxLength(220);
        builder.HasIndex(p => p.Slug).IsUnique();

        builder.Property(p => p.Description).IsRequired().HasMaxLength(4000);

        builder.Property(p => p.Price).IsRequired().HasColumnType("decimal(10,2)");
        builder.Property(p => p.DiscountPrice).HasColumnType("decimal(10,2)");
        builder.Property(p => p.Stock).IsRequired();

        builder.Property(p => p.IsFeatured).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.MakingProcessVideoUrl).HasMaxLength(2000);

        builder.Property(p => p.ViewCount).IsRequired();
        builder.Property(p => p.SalesCount).IsRequired();
        builder.Property(p => p.AverageRating).IsRequired().HasColumnType("decimal(3,2)");
        builder.Property(p => p.ReviewCount).IsRequired();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => new { p.IsActive, p.CategoryId });
        builder.HasIndex(p => new { p.IsActive, p.DistrictId });
        builder.HasIndex(p => new { p.IsActive, p.IsFeatured });
        builder.HasIndex(p => p.ProducerId);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.District)
            .WithMany(d => d.Products)
            .HasForeignKey(p => p.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Producer)
            .WithMany()
            .HasForeignKey(p => p.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
