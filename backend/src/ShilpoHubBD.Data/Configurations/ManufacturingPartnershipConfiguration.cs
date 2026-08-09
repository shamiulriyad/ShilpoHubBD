using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Data.Configurations;

public class ManufacturingPartnershipConfiguration : IEntityTypeConfiguration<ManufacturingPartnership>
{
    public void Configure(EntityTypeBuilder<ManufacturingPartnership> builder)
    {
        builder.ToTable("ManufacturingPartnerships");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ReferenceNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.ProductRequirements).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.ManufacturingSpecifications).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.TargetUnitPrice).HasColumnType("numeric(18,2)");
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ProducerResponseNotes).HasMaxLength(1000);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.ReferenceNumber).IsUnique();
        builder.HasIndex(p => p.BusinessPartnerId);
        builder.HasIndex(p => p.ProducerId);

        builder.HasOne(p => p.BusinessPartner)
            .WithMany()
            .HasForeignKey(p => p.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Producer)
            .WithMany()
            .HasForeignKey(p => p.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Milestones)
            .WithOne(m => m.Partnership)
            .HasForeignKey(m => m.PartnershipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StatusHistory)
            .WithOne(h => h.Partnership)
            .HasForeignKey(h => h.PartnershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
