using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Traceability;

namespace ShilpoHubBD.Data.Configurations;

public class ProductTraceabilityConfiguration : IEntityTypeConfiguration<ProductTraceability>
{
    public void Configure(EntityTypeBuilder<ProductTraceability> builder)
    {
        builder.ToTable("ProductTraceabilities");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.HasIndex(t => t.ProductId).IsUnique();

        builder.HasOne(t => t.Product)
            .WithMany()
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.MaterialSources)
            .WithOne(m => m.ProductTraceability)
            .HasForeignKey(m => m.ProductTraceabilityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.TimelineEvents)
            .WithOne(e => e.ProductTraceability)
            .HasForeignKey(e => e.ProductTraceabilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
