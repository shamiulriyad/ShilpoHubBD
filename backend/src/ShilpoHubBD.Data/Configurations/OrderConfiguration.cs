using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.Property(o => o.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.PaymentMethod).IsRequired().HasConversion<string>().HasMaxLength(30);

        builder.Property(o => o.Subtotal).IsRequired().HasColumnType("decimal(12,2)");
        builder.Property(o => o.Total).IsRequired().HasColumnType("decimal(12,2)");

        builder.Property(o => o.RecipientName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.RecipientPhone).IsRequired().HasMaxLength(20);
        builder.Property(o => o.ShippingAddressLine).IsRequired().HasMaxLength(500);

        builder.Property(o => o.TrackingNumber).HasMaxLength(100);
        builder.Property(o => o.Carrier).HasMaxLength(100);
        builder.Property(o => o.CancelReason).HasMaxLength(500);
        builder.Property(o => o.ReturnReason).HasMaxLength(500);
        builder.Property(o => o.RefundAmount).HasColumnType("decimal(12,2)");
        builder.Property(o => o.RefundReason).HasMaxLength(500);

        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        builder.HasIndex(o => new { o.UserId, o.CreatedAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ShippingDistrict)
            .WithMany()
            .HasForeignKey(o => o.ShippingDistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
