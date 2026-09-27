using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerPartnershipSettlementConfiguration : IEntityTypeConfiguration<ProducerPartnershipSettlement>
{
    public void Configure(EntityTypeBuilder<ProducerPartnershipSettlement> builder)
    {
        builder.ToTable("ProducerPartnershipSettlements");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.GrossRevenue).HasColumnType("numeric(18,2)");
        builder.Property(s => s.RefundDeductions).HasColumnType("numeric(18,2)");
        builder.Property(s => s.PlatformFeePercentageApplied).HasColumnType("numeric(5,2)");
        builder.Property(s => s.PlatformFeeAmount).HasColumnType("numeric(18,2)");
        builder.Property(s => s.NetPartnershipRevenue).HasColumnType("numeric(18,2)");
        builder.Property(s => s.ProducerSharePercentageApplied).HasColumnType("numeric(5,2)");
        builder.Property(s => s.BusinessPartnerSharePercentageApplied).HasColumnType("numeric(5,2)");
        builder.Property(s => s.ProducerShareAmount).HasColumnType("numeric(18,2)");
        builder.Property(s => s.BusinessPartnerShareAmount).HasColumnType("numeric(18,2)");
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.PayoutReference).HasMaxLength(200);
        builder.Property(s => s.RejectionReason).HasMaxLength(1000);
        builder.Property(s => s.CalculatedAt).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.AgreementId);
        builder.HasIndex(s => s.Status);
        // A period can be recalculated after a rejection, but two non-rejected settlements must
        // never cover the same agreement — the service enforces the overlap rule; this composite
        // index just makes the (agreement, period) lookup that check relies on efficient.
        builder.HasIndex(s => new { s.AgreementId, s.PeriodStart, s.PeriodEnd });

        builder.HasOne(s => s.Agreement)
            .WithMany(a => a.Settlements)
            .HasForeignKey(s => s.AgreementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ApprovedBy)
            .WithMany()
            .HasForeignKey(s => s.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
