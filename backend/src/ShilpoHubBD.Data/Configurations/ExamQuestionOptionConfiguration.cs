using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Data.Configurations;

public class ExamQuestionOptionConfiguration : IEntityTypeConfiguration<ExamQuestionOption>
{
    public void Configure(EntityTypeBuilder<ExamQuestionOption> builder)
    {
        builder.ToTable("ExamQuestionOptions");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Text).IsRequired().HasMaxLength(1000);
        builder.Property(o => o.DisplayOrder).IsRequired();

        builder.HasIndex(o => o.QuestionId);
    }
}
