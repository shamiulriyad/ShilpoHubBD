using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Data.Configurations;

public class SurveyQuestionConfiguration : IEntityTypeConfiguration<SurveyQuestion>
{
    public void Configure(EntityTypeBuilder<SurveyQuestion> builder)
    {
        builder.ToTable("SurveyQuestions");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Text).IsRequired().HasMaxLength(1000);
        builder.Property(q => q.HelpText).HasMaxLength(1000);
        builder.Property(q => q.QuestionType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(q => q.OptionsJson).HasMaxLength(4000);
        builder.Property(q => q.CreatedAt).IsRequired();
        builder.Property(q => q.UpdatedAt).IsRequired();

        builder.HasIndex(q => new { q.SurveyId, q.OrderIndex });

        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.SurveyQuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
