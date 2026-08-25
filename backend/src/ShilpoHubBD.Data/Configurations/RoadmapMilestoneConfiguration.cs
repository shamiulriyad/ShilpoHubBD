using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Roadmap;

namespace ShilpoHubBD.Data.Configurations;

public class RoadmapMilestoneConfiguration : IEntityTypeConfiguration<RoadmapMilestone>
{
    public void Configure(EntityTypeBuilder<RoadmapMilestone> builder)
    {
        builder.ToTable("RoadmapMilestones");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.TargetLevel).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.DisplayOrder).IsRequired();

        builder.HasIndex(m => new { m.LearningRoadmapId, m.DisplayOrder });
        builder.HasIndex(m => m.HeritageSkillId);

        builder.HasOne(m => m.HeritageSkill)
            .WithMany()
            .HasForeignKey(m => m.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.RecommendedCourses)
            .WithOne(c => c.RoadmapMilestone)
            .HasForeignKey(c => c.RoadmapMilestoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.RecommendedLessons)
            .WithOne(l => l.RoadmapMilestone)
            .HasForeignKey(l => l.RoadmapMilestoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
