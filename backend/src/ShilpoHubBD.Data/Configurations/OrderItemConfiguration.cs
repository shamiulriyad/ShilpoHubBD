using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.ProductImageUrl).HasMaxLength(2000);
        builder.Property(i => i.VariantName).HasMaxLength(200);

        builder.Property(i => i.UnitPrice).IsRequired().HasColumnType("decimal(10,2)");
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.LineTotal).IsRequired().HasColumnType("decimal(12,2)");

        builder.Property(i => i.ProducerStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.ProducerNote).HasMaxLength(1000);
        builder.Property(i => i.TrackingNumber).HasMaxLength(100);
        builder.Property(i => i.Carrier).HasMaxLength(100);

        builder.HasIndex(i => i.OrderId);
        builder.HasIndex(i => i.ProducerStatus);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
