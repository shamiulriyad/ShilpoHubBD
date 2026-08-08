using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Configurations;

public class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
{
    public void Configure(EntityTypeBuilder<CourseEnrollment> builder)
    {
        builder.ToTable("CourseEnrollments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.EnrolledAt).IsRequired();

        builder.HasIndex(e => new { e.CourseId, e.ApprenticeId }).IsUnique();
        builder.HasIndex(e => e.ApprenticeId);

        builder.HasOne(e => e.Apprentice)
            .WithMany()
            .HasForeignKey(e => e.ApprenticeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.LessonProgress)
            .WithOne(p => p.Enrollment)
            .HasForeignKey(p => p.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
