using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Data.Configurations;

public class ExamAttemptConfiguration : IEntityTypeConfiguration<ExamAttempt>
{
    public void Configure(EntityTypeBuilder<ExamAttempt> builder)
    {
        builder.ToTable("ExamAttempts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.PercentageScore).HasColumnType("decimal(5,2)");
        builder.Property(a => a.StartedAt).IsRequired();
        builder.Property(a => a.MaxScore).IsRequired();

        builder.HasIndex(a => new { a.ExamId, a.StudentUserId });
        builder.HasIndex(a => a.StudentUserId);

        builder.HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Answers)
            .WithOne(ans => ans.ExamAttempt)
            .HasForeignKey(ans => ans.ExamAttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
