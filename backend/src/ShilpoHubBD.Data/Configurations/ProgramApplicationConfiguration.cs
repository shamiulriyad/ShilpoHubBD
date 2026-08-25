using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Data.Configurations;

public class ProgramApplicationConfiguration : IEntityTypeConfiguration<ProgramApplication>
{
    public void Configure(EntityTypeBuilder<ProgramApplication> builder)
    {
        builder.ToTable("ProgramApplications");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Message).IsRequired().HasMaxLength(2000);
        builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.ResponseMessage).HasMaxLength(2000);
        builder.Property(a => a.AppliedAt).IsRequired();

        builder.HasIndex(a => new { a.ProgramId, a.Status });
        builder.HasIndex(a => new { a.ApplicantUserId, a.Status });

        // Backstops the application-layer duplicate check against a race between two concurrent
        // requests; still allows re-applying after a prior application was rejected/withdrawn.
        builder.HasIndex(a => new { a.ProgramId, a.ApplicantUserId })
            .IsUnique()
            .HasFilter("\"Status\" IN ('Pending', 'Accepted')");

        builder.HasOne(a => a.Applicant)
            .WithMany()
            .HasForeignKey(a => a.ApplicantUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
