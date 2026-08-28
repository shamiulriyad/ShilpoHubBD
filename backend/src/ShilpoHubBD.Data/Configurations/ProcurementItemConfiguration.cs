using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Procurement;

namespace ShilpoHubBD.Data.Configurations;

public class ProcurementItemConfiguration : IEntityTypeConfiguration<ProcurementItem>
{
    public void Configure(EntityTypeBuilder<ProcurementItem> builder)
    {
        builder.ToTable("ProcurementItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.UnitPrice).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(i => i.Specifications).HasMaxLength(2000);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
