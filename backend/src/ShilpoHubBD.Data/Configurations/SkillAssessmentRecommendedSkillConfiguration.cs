using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.SkillAssessment;

namespace ShilpoHubBD.Data.Configurations;

public class SkillAssessmentRecommendedSkillConfiguration : IEntityTypeConfiguration<SkillAssessmentRecommendedSkill>
{
    public void Configure(EntityTypeBuilder<SkillAssessmentRecommendedSkill> builder)
    {
        builder.ToTable("SkillAssessmentRecommendedSkills");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Reason).IsRequired().HasMaxLength(1000);

        builder.HasIndex(r => r.SkillAssessmentId);

        builder.HasOne(r => r.HeritageSkill)
            .WithMany()
            .HasForeignKey(r => r.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
