using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Configurations;

public class TrainingCertificateConfiguration : IEntityTypeConfiguration<TrainingCertificate>
{
    public void Configure(EntityTypeBuilder<TrainingCertificate> builder)
    {
        builder.ToTable("TrainingCertificates");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CertificateNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.CourseTitle).IsRequired().HasMaxLength(200);
        builder.Property(c => c.ApprenticeName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.MentorName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.IssuedAt).IsRequired();

        builder.HasIndex(c => c.CertificateNumber).IsUnique();
        builder.HasIndex(c => c.EnrollmentId).IsUnique();

        builder.HasOne(c => c.Enrollment)
            .WithMany()
            .HasForeignKey(c => c.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
