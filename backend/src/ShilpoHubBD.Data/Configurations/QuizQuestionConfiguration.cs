using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Data.Configurations;

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("QuizQuestions");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Body).IsRequired().HasMaxLength(2000);
        builder.Property(q => q.Points).IsRequired();
        builder.Property(q => q.DisplayOrder).IsRequired();

        builder.HasIndex(q => new { q.QuizId, q.DisplayOrder });

        builder.HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
