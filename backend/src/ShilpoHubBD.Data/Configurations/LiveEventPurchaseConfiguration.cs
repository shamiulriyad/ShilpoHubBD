using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.LiveShopping;

namespace ShilpoHubBD.Data.Configurations;

public class LiveEventPurchaseConfiguration : IEntityTypeConfiguration<LiveEventPurchase>
{
    public void Configure(EntityTypeBuilder<LiveEventPurchase> builder)
    {
        builder.ToTable("LiveEventPurchases");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UnitPrice).IsRequired().HasColumnType("decimal(12,2)");
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => p.LiveEventId);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Product)
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
