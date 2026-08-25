using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Employment;

namespace ShilpoHubBD.Data.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.CoverMessage).IsRequired().HasMaxLength(2000);
        builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.ResponseMessage).HasMaxLength(2000);
        builder.Property(a => a.AppliedAt).IsRequired();

        builder.HasIndex(a => new { a.JobListingId, a.Status });
        builder.HasIndex(a => new { a.ApplicantUserId, a.Status });

        builder.HasOne(a => a.Applicant)
            .WithMany()
            .HasForeignKey(a => a.ApplicantUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
