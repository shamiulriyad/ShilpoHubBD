using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.SkillAssessment;

namespace ShilpoHubBD.Data.Configurations;

public class SkillAssessmentConfiguration : IEntityTypeConfiguration<SkillAssessment>
{
    public void Configure(EntityTypeBuilder<SkillAssessment> builder)
    {
        builder.ToTable("SkillAssessments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Level).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Score).IsRequired().HasColumnType("decimal(5,2)");
        builder.Property(a => a.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(a => a.AssessedAt).IsRequired();

        builder.HasIndex(a => a.AcademyMemberProfileId);
        builder.HasIndex(a => a.HeritageSkillId);

        builder.HasOne(a => a.AcademyMemberProfile)
            .WithMany()
            .HasForeignKey(a => a.AcademyMemberProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.HeritageSkill)
            .WithMany()
            .HasForeignKey(a => a.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Insights)
            .WithOne(i => i.SkillAssessment)
            .HasForeignKey(i => i.SkillAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.RecommendedSkills)
            .WithOne(r => r.SkillAssessment)
            .HasForeignKey(r => r.SkillAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
