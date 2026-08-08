using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Auction;

namespace ShilpoHubBD.Data.Configurations;

public class AuctionBidConfiguration : IEntityTypeConfiguration<AuctionBid>
{
    public void Configure(EntityTypeBuilder<AuctionBid> builder)
    {
        builder.ToTable("AuctionBids");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount).IsRequired().HasColumnType("decimal(12,2)");
        builder.Property(b => b.CreatedAt).IsRequired();

        builder.HasIndex(b => b.AuctionId);
        builder.HasIndex(b => new { b.AuctionId, b.Amount });

        builder.HasOne(b => b.Bidder)
            .WithMany()
            .HasForeignKey(b => b.BidderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
