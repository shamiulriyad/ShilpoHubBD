using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Provider).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Amount).IsRequired().HasColumnType("decimal(12,2)");
        builder.Property(p => p.RefundedAmount).IsRequired().HasColumnType("decimal(12,2)");
        builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.TransactionReference).HasMaxLength(200);
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.RefundReason).HasMaxLength(500);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.OrderId);

        builder.HasOne(p => p.Order)
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
