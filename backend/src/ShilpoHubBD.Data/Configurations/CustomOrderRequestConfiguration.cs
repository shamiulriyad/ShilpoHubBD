using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.CustomOrders;

namespace ShilpoHubBD.Data.Configurations;

public class CustomOrderRequestConfiguration : IEntityTypeConfiguration<CustomOrderRequest>
{
    public void Configure(EntityTypeBuilder<CustomOrderRequest> builder)
    {
        builder.ToTable("CustomOrderRequests");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Specifications).IsRequired().HasMaxLength(4000);
        builder.Property(c => c.Budget).HasColumnType("decimal(10,2)");
        builder.Property(c => c.QuotedPrice).HasColumnType("decimal(10,2)");
        builder.Property(c => c.ProducerResponse).HasMaxLength(2000);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasIndex(c => c.ProducerId);
        builder.HasIndex(c => c.CustomerId);
        builder.HasIndex(c => c.ProductId);

        builder.HasOne(c => c.Producer)
            .WithMany()
            .HasForeignKey(c => c.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
