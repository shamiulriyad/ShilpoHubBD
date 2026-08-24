using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Data.Configurations;

public class ExamAttemptAnswerConfiguration : IEntityTypeConfiguration<ExamAttemptAnswer>
{
    public void Configure(EntityTypeBuilder<ExamAttemptAnswer> builder)
    {
        builder.ToTable("ExamAttemptAnswers");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EssayAnswerText).HasMaxLength(8000);
        builder.Property(a => a.Feedback).HasMaxLength(2000);

        builder.HasIndex(a => new { a.ExamAttemptId, a.ExamQuestionId }).IsUnique();

        builder.HasOne(a => a.ExamQuestion)
            .WithMany()
            .HasForeignKey(a => a.ExamQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.SelectedOption)
            .WithMany()
            .HasForeignKey(a => a.SelectedOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
