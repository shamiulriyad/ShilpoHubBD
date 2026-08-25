using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Data.Configurations;

public class ApprenticeMilestoneProgressConfiguration : IEntityTypeConfiguration<ApprenticeMilestoneProgress>
{
    public void Configure(EntityTypeBuilder<ApprenticeMilestoneProgress> builder)
    {
        builder.ToTable("ApprenticeMilestoneProgress");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Notes).HasMaxLength(2000);

        builder.HasIndex(p => new { p.EnrollmentId, p.MilestoneId }).IsUnique();

        builder.HasOne(p => p.Milestone)
            .WithMany()
            .HasForeignKey(p => p.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
