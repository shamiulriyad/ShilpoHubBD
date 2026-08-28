using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchMilestoneConfiguration : IEntityTypeConfiguration<ResearchMilestone>
{
    public void Configure(EntityTypeBuilder<ResearchMilestone> builder)
    {
        builder.ToTable("ResearchMilestones");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(4000);
        builder.Property(m => m.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();

        builder.HasIndex(m => new { m.ResearchProjectId, m.OrderIndex });
        builder.HasIndex(m => m.Status);
    }
}
