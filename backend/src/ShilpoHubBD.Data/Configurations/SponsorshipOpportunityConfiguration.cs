using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Data.Configurations;

public class SponsorshipOpportunityConfiguration : IEntityTypeConfiguration<SponsorshipOpportunity>
{
    public void Configure(EntityTypeBuilder<SponsorshipOpportunity> builder)
    {
        builder.ToTable("SponsorshipOpportunities");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Description).IsRequired().HasMaxLength(4000);
        builder.Property(o => o.FundingGoal).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        builder.HasIndex(o => o.ProducerId);

        builder.HasOne(o => o.Producer)
            .WithMany()
            .HasForeignKey(o => o.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Proposals)
            .WithOne(p => p.Opportunity)
            .HasForeignKey(p => p.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
