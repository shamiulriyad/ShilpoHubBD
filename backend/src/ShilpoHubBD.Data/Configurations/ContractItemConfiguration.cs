using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Data.Configurations;

public class ContractItemConfiguration : IEntityTypeConfiguration<ContractItem>
{
    public void Configure(EntityTypeBuilder<ContractItem> builder)
    {
        builder.ToTable("ContractItems");
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
