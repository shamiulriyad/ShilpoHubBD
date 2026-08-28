using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Data.Configurations;

public class SponsorshipProposalConfiguration : IEntityTypeConfiguration<SponsorshipProposal>
{
    public void Configure(EntityTypeBuilder<SponsorshipProposal> builder)
    {
        builder.ToTable("SponsorshipProposals");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FundingAmount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(p => p.ProposalMessage).HasMaxLength(2000);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.DecisionNotes).HasMaxLength(1000);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.OpportunityId);
        builder.HasIndex(p => p.BusinessPartnerId);

        builder.HasOne(p => p.BusinessPartner)
            .WithMany()
            .HasForeignKey(p => p.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Milestones)
            .WithOne(m => m.Proposal)
            .HasForeignKey(m => m.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ProgressUpdates)
            .WithOne(u => u.Proposal)
            .HasForeignKey(u => u.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ImpactRecords)
            .WithOne(r => r.Proposal)
            .HasForeignKey(r => r.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StatusHistory)
            .WithOne(h => h.Proposal)
            .HasForeignKey(h => h.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
