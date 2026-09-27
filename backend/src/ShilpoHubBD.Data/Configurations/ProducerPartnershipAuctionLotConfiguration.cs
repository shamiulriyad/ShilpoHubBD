using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerPartnershipAuctionLotConfiguration : IEntityTypeConfiguration<ProducerPartnershipAuctionLot>
{
    public void Configure(EntityTypeBuilder<ProducerPartnershipAuctionLot> builder)
    {
        builder.ToTable("ProducerPartnershipAuctionLots");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.StartingBid).HasColumnType("numeric(18,2)");
        builder.Property(l => l.CurrentHighestBid).HasColumnType("numeric(18,2)");
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.UpdatedAt).IsRequired();

        // One lot per producer per auction round.
        builder.HasIndex(l => new { l.AuctionId, l.ProducerId }).IsUnique();
        builder.HasIndex(l => l.Status);

        builder.HasOne(l => l.Auction)
            .WithMany(a => a.Lots)
            .HasForeignKey(l => l.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Producer)
            .WithMany()
            .HasForeignKey(l => l.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.CurrentHighestBidder)
            .WithMany()
            .HasForeignKey(l => l.CurrentHighestBidderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Bids)
            .WithOne(b => b.Lot)
            .HasForeignKey(b => b.LotId)
            .OnDelete(DeleteBehavior.Cascade);

        // The winning bid is one of this lot's own bids — no separate delete path needed, and
        // pointing it at Restrict (instead of Cascade, which EF would reject as a second cascade
        // path from Lot -> Bids -> this same Bid) avoids a multiple-cascade-paths error.
        builder.HasOne(l => l.WinningBid)
            .WithMany()
            .HasForeignKey(l => l.WinningBidId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
