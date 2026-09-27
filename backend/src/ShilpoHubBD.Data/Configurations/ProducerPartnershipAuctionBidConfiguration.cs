using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerPartnershipAuctionBidConfiguration : IEntityTypeConfiguration<ProducerPartnershipAuctionBid>
{
    public void Configure(EntityTypeBuilder<ProducerPartnershipAuctionBid> builder)
    {
        builder.ToTable("ProducerPartnershipAuctionBids");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount).HasColumnType("numeric(18,2)");
        builder.Property(b => b.PlacedAt).IsRequired();

        builder.HasIndex(b => b.LotId);
        builder.HasIndex(b => b.BusinessPartnerId);
        builder.HasIndex(b => new { b.LotId, b.Amount });

        builder.HasOne(b => b.BusinessPartner)
            .WithMany()
            .HasForeignKey(b => b.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
