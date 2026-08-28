using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Data.Configurations;

public class InvestmentMilestoneConfiguration : IEntityTypeConfiguration<InvestmentMilestone>
{
    public void Configure(EntityTypeBuilder<InvestmentMilestone> builder)
    {
        builder.ToTable("InvestmentMilestones");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(2000);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
    }
}
