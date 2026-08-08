using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Sustainability;

namespace ShilpoHubBD.Data.Configurations;

public class SustainableMaterialRecordConfiguration : IEntityTypeConfiguration<SustainableMaterialRecord>
{
    public void Configure(EntityTypeBuilder<SustainableMaterialRecord> builder)
    {
        builder.ToTable("SustainableMaterialRecords");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.MaterialName).IsRequired().HasMaxLength(200);
        builder.Property(r => r.QuantityUsed).IsRequired().HasColumnType("decimal(12,2)");
        builder.Property(r => r.Unit).IsRequired().HasMaxLength(30);
        builder.Property(r => r.CarbonSavingsPerUnitKg).IsRequired().HasColumnType("decimal(10,2)");
        builder.Property(r => r.RecordedAt).IsRequired();

        builder.HasIndex(r => r.SustainabilityProfileId);

        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
