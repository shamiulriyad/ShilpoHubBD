using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Roadmap;

namespace ShilpoHubBD.Data.Configurations;

public class LearningRoadmapConfiguration : IEntityTypeConfiguration<LearningRoadmap>
{
    public void Configure(EntityTypeBuilder<LearningRoadmap> builder)
    {
        builder.ToTable("LearningRoadmaps");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Goal).IsRequired().HasMaxLength(1000);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.GeneratedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => new { r.AcademyMemberProfileId, r.Status });

        builder.HasOne(r => r.AcademyMemberProfile)
            .WithMany()
            .HasForeignKey(r => r.AcademyMemberProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.TargetHeritageSkill)
            .WithMany()
            .HasForeignKey(r => r.TargetHeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Milestones)
            .WithOne(m => m.LearningRoadmap)
            .HasForeignKey(m => m.LearningRoadmapId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
