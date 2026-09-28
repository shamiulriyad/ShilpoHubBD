using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerPartnershipAuctionParticipantConfiguration : IEntityTypeConfiguration<ProducerPartnershipAuctionParticipant>
{
    public void Configure(EntityTypeBuilder<ProducerPartnershipAuctionParticipant> builder)
    {
        builder.ToTable("ProducerPartnershipAuctionParticipants");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.DecisionNotes).HasMaxLength(1000);
        builder.Property(p => p.AppliedAt).IsRequired();

        // One registration per Business Partner per auction round.
        builder.HasIndex(p => new { p.AuctionId, p.BusinessPartnerId }).IsUnique();
        builder.HasIndex(p => p.Status);

        builder.HasOne(p => p.Auction)
            .WithMany(a => a.Participants)
            .HasForeignKey(p => p.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.BusinessPartner)
            .WithMany()
            .HasForeignKey(p => p.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.DecidedBy)
            .WithMany()
            .HasForeignKey(p => p.DecidedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
