using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Data.Configurations;

public class TrainingMilestoneConfiguration : IEntityTypeConfiguration<TrainingMilestone>
{
    public void Configure(EntityTypeBuilder<TrainingMilestone> builder)
    {
        builder.ToTable("TrainingMilestones");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000);
        builder.Property(m => m.CreatedAt).IsRequired();

        builder.HasIndex(m => m.ProgramId);
    }
}
