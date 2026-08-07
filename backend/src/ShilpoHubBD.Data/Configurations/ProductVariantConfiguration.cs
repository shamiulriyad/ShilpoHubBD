using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Sku).HasMaxLength(100);
        builder.Property(v => v.Price).HasColumnType("decimal(10,2)");
        builder.Property(v => v.Stock).IsRequired();
        builder.Property(v => v.DisplayOrder).IsRequired();
        builder.Property(v => v.IsActive).IsRequired();

        builder.HasIndex(v => v.ProductId);
    }
}
