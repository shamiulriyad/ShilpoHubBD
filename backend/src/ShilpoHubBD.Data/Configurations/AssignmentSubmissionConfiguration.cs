using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Data.Configurations;

public class AssignmentSubmissionConfiguration : IEntityTypeConfiguration<AssignmentSubmission>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmission> builder)
    {
        builder.ToTable("AssignmentSubmissions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SubmissionText).IsRequired().HasMaxLength(8000);
        builder.Property(s => s.AttachmentUrl).HasMaxLength(2000);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Feedback).HasMaxLength(2000);
        builder.Property(s => s.SubmittedAt).IsRequired();

        builder.HasIndex(s => new { s.AssignmentId, s.StudentUserId }).IsUnique();
        builder.HasIndex(s => s.StudentUserId);

        builder.HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
