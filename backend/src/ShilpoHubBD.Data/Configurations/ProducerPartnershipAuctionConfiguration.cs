using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerPartnershipAuctionConfiguration : IEntityTypeConfiguration<ProducerPartnershipAuction>
{
    public void Configure(EntityTypeBuilder<ProducerPartnershipAuction> builder)
    {
        builder.ToTable("ProducerPartnershipAuctions");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Slug).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(2000);
        builder.Property(a => a.EligibilityCriteria).HasMaxLength(2000);
        builder.Property(a => a.BusinessPartnerEligibilityCriteria).HasMaxLength(2000);
        builder.Property(a => a.ParticipationFee).HasColumnType("numeric(18,2)");
        builder.Property(a => a.Currency).IsRequired().HasMaxLength(10);
        builder.Property(a => a.MinimumStartingBid).HasColumnType("numeric(18,2)");
        builder.Property(a => a.MinimumBidIncrement).HasColumnType("numeric(18,2)");
        builder.Property(a => a.DefaultRevenueSharePercentage).HasColumnType("numeric(5,2)");
        builder.Property(a => a.SettlementRulesDescription).HasMaxLength(4000);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        builder.HasIndex(a => a.Slug).IsUnique();
        builder.HasIndex(a => a.AuctionYear);
        builder.HasIndex(a => a.Status);

        builder.HasOne(a => a.ManagedBy)
            .WithMany()
            .HasForeignKey(a => a.ManagedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Agreements)
            .WithOne(g => g.Auction)
            .HasForeignKey(g => g.AuctionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
