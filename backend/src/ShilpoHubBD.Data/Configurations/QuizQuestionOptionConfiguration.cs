using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Data.Configurations;

public class QuizQuestionOptionConfiguration : IEntityTypeConfiguration<QuizQuestionOption>
{
    public void Configure(EntityTypeBuilder<QuizQuestionOption> builder)
    {
        builder.ToTable("QuizQuestionOptions");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Text).IsRequired().HasMaxLength(1000);
        builder.Property(o => o.DisplayOrder).IsRequired();

        builder.HasIndex(o => o.QuestionId);
    }
}
