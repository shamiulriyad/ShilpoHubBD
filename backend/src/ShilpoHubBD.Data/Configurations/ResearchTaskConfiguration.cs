using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchTaskConfiguration : IEntityTypeConfiguration<ResearchTask>
{
    public void Configure(EntityTypeBuilder<ResearchTask> builder)
    {
        builder.ToTable("ResearchTasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(4000);
        builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Priority).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.HasIndex(t => t.ResearchProjectId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.AssignedToUserId);
        builder.HasIndex(t => t.MilestoneId);

        builder.HasOne(t => t.Milestone)
            .WithMany(m => m.Tasks)
            .HasForeignKey(t => t.MilestoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
