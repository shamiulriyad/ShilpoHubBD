using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.SkillAssessment;

namespace ShilpoHubBD.Data.Configurations;

public class SkillAssessmentInsightConfiguration : IEntityTypeConfiguration<SkillAssessmentInsight>
{
    public void Configure(EntityTypeBuilder<SkillAssessmentInsight> builder)
    {
        builder.ToTable("SkillAssessmentInsights");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Text).IsRequired().HasMaxLength(1000);

        builder.HasIndex(i => new { i.SkillAssessmentId, i.Type, i.DisplayOrder });
    }
}
