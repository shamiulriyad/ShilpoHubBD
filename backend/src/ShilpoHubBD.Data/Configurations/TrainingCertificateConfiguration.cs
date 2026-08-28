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

        builder.Property(c => c.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.CertificateNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.RecipientName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.IssuerName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.IssuedAt).IsRequired();

        builder.HasIndex(c => c.CertificateNumber).IsUnique();
        builder.HasIndex(c => c.RecipientUserId);
        builder.HasIndex(c => c.EnrollmentId).IsUnique().HasFilter("\"EnrollmentId\" IS NOT NULL");
        builder.HasIndex(c => c.ApprenticeEnrollmentId).IsUnique().HasFilter("\"ApprenticeEnrollmentId\" IS NOT NULL");
        builder.HasIndex(c => new { c.RecipientUserId, c.HeritageSkillId, c.Type });

        builder.HasOne(c => c.Recipient)
            .WithMany()
            .HasForeignKey(c => c.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Issuer)
            .WithMany()
            .HasForeignKey(c => c.IssuerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Enrollment)
            .WithMany()
            .HasForeignKey(c => c.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ApprenticeEnrollment)
            .WithMany()
            .HasForeignKey(c => c.ApprenticeEnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.HeritageSkill)
            .WithMany()
            .HasForeignKey(c => c.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
