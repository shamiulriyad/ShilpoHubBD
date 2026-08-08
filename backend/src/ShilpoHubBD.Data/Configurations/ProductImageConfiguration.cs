using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl).IsRequired().HasMaxLength(2000);
        builder.Property(i => i.DisplayOrder).IsRequired();
        builder.Property(i => i.IsPrimary).IsRequired();
        builder.Property(i => i.ImageType).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(i => i.ProductId);
    }
}
