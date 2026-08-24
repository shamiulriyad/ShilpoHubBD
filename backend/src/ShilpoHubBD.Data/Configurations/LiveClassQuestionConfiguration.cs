using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.LiveClass;

namespace ShilpoHubBD.Data.Configurations;

public class LiveClassQuestionConfiguration : IEntityTypeConfiguration<LiveClassQuestion>
{
    public void Configure(EntityTypeBuilder<LiveClassQuestion> builder)
    {
        builder.ToTable("LiveClassQuestions");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Body).IsRequired().HasMaxLength(1000);
        builder.Property(q => q.AnswerBody).HasMaxLength(2000);
        builder.Property(q => q.CreatedAt).IsRequired();

        builder.HasIndex(q => q.LiveClassId);

        builder.HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
