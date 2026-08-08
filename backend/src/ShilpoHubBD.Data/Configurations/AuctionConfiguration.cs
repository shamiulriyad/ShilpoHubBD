using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Auction;

namespace ShilpoHubBD.Data.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.ToTable("Auctions");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(2000);
        builder.Property(a => a.StartingPrice).IsRequired().HasColumnType("decimal(12,2)");
        builder.Property(a => a.CurrentPrice).IsRequired().HasColumnType("decimal(12,2)");
        builder.Property(a => a.MinBidIncrement).IsRequired().HasColumnType("decimal(12,2)");
        builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.StartAt).IsRequired();
        builder.Property(a => a.EndAt).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.ProducerId);
        builder.HasIndex(a => a.EndAt);

        builder.HasOne(a => a.Producer)
            .WithMany()
            .HasForeignKey(a => a.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Product)
            .WithMany()
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Winner)
            .WithMany()
            .HasForeignKey(a => a.WinnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Bids)
            .WithOne(b => b.Auction)
            .HasForeignKey(b => b.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
