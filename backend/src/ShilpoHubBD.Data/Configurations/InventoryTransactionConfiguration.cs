using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Inventory;

namespace ShilpoHubBD.Data.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Reason).IsRequired().HasMaxLength(500);
        builder.Property(t => t.ChangeAmount).IsRequired();
        builder.Property(t => t.PreviousStock).IsRequired();
        builder.Property(t => t.NewStock).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasIndex(t => t.ProductId);
        builder.HasIndex(t => t.ProductVariantId);

        builder.HasOne(t => t.Product)
            .WithMany()
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.ProductVariant)
            .WithMany()
            .HasForeignKey(t => t.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
