using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Configurations;

public class ProductVideoConfiguration : IEntityTypeConfiguration<ProductVideo>
{
    public void Configure(EntityTypeBuilder<ProductVideo> builder)
    {
        builder.ToTable("ProductVideos");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.VideoUrl).IsRequired().HasMaxLength(2000);
        builder.Property(v => v.Title).HasMaxLength(200);
        builder.Property(v => v.DisplayOrder).IsRequired();
        builder.Property(v => v.CreatedAt).IsRequired();

        builder.HasIndex(v => v.ProductId);
    }
}
