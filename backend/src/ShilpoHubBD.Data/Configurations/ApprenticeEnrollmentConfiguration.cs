using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Data.Configurations;

public class ApprenticeEnrollmentConfiguration : IEntityTypeConfiguration<ApprenticeEnrollment>
{
    public void Configure(EntityTypeBuilder<ApprenticeEnrollment> builder)
    {
        builder.ToTable("ApprenticeEnrollments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.EnrolledAt).IsRequired();

        builder.HasIndex(e => new { e.ProgramId, e.ApprenticeUserId }).IsUnique();
        builder.HasIndex(e => e.ApprenticeUserId);

        builder.HasOne(e => e.Apprentice)
            .WithMany()
            .HasForeignKey(e => e.ApprenticeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Application)
            .WithMany()
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.MilestoneProgress)
            .WithOne(p => p.Enrollment)
            .HasForeignKey(p => p.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
