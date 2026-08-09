using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Data.Configurations;

public class ProductDevelopmentStatusEventConfiguration : IEntityTypeConfiguration<ProductDevelopmentStatusEvent>
{
    public void Configure(EntityTypeBuilder<ProductDevelopmentStatusEvent> builder)
    {
        builder.ToTable("ProductDevelopmentStatusEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
