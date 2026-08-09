using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Data.Configurations;

public class QuotationRequestItemConfiguration : IEntityTypeConfiguration<QuotationRequestItem>
{
    public void Configure(EntityTypeBuilder<QuotationRequestItem> builder)
    {
        builder.ToTable("QuotationRequestItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.TargetPrice).HasColumnType("numeric(18,2)");
        builder.Property(i => i.Specifications).HasMaxLength(2000);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Category)
            .WithMany()
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
