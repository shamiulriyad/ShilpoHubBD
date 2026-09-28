using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerPartnershipAgreementConfiguration : IEntityTypeConfiguration<ProducerPartnershipAgreement>
{
    public void Configure(EntityTypeBuilder<ProducerPartnershipAgreement> builder)
    {
        builder.ToTable("ProducerPartnershipAgreements");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(g => g.WinningBidAmount).HasColumnType("numeric(18,2)");
        builder.Property(g => g.ProducerSharePercentage).HasColumnType("numeric(5,2)");
        builder.Property(g => g.BusinessPartnerSharePercentage).HasColumnType("numeric(5,2)");
        builder.Property(g => g.PlatformFeePercentage).HasColumnType("numeric(5,2)");
        builder.Property(g => g.SettlementFrequency).HasConversion<string>().HasMaxLength(20);
        builder.Property(g => g.MinimumSettlementAmount).HasColumnType("numeric(18,2)");
        builder.Property(g => g.AgreementTerms).HasMaxLength(4000);
        builder.Property(g => g.EndReason).HasMaxLength(1000);
        builder.Property(g => g.CreatedAt).IsRequired();
        builder.Property(g => g.UpdatedAt).IsRequired();

        builder.HasIndex(g => g.ProducerId);
        builder.HasIndex(g => g.BusinessPartnerId);
        builder.HasIndex(g => g.AuctionId);
        builder.HasIndex(g => g.AuctionLotId).IsUnique();
        builder.HasIndex(g => g.Status);

        builder.HasOne(g => g.Producer)
            .WithMany()
            .HasForeignKey(g => g.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.BusinessPartner)
            .WithMany()
            .HasForeignKey(g => g.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.AuctionLot)
            .WithMany()
            .HasForeignKey(g => g.AuctionLotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.StatusHistory)
            .WithOne(h => h.Agreement)
            .HasForeignKey(h => h.AgreementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
