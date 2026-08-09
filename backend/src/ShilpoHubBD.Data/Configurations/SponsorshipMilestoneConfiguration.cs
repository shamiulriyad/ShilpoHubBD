using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Data.Configurations;

public class SponsorshipMilestoneConfiguration : IEntityTypeConfiguration<SponsorshipMilestone>
{
    public void Configure(EntityTypeBuilder<SponsorshipMilestone> builder)
    {
        builder.ToTable("SponsorshipMilestones");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(2000);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
    }
}
