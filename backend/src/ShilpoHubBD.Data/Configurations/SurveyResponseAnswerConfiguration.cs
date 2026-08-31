using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Data.Configurations;

public class SurveyResponseAnswerConfiguration : IEntityTypeConfiguration<SurveyResponseAnswer>
{
    public void Configure(EntityTypeBuilder<SurveyResponseAnswer> builder)
    {
        builder.ToTable("SurveyResponseAnswers");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ValueText).HasMaxLength(8000);

        builder.HasIndex(a => new { a.SurveyResponseId, a.SurveyQuestionId }).IsUnique();
        builder.HasIndex(a => a.SurveyQuestionId);
    }
}
