using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Employment;

namespace ShilpoHubBD.Data.Configurations;

public class JobListingConfiguration : IEntityTypeConfiguration<JobListing>
{
    public void Configure(EntityTypeBuilder<JobListing> builder)
    {
        builder.ToTable("JobListings");
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title).IsRequired().HasMaxLength(200);
        builder.Property(j => j.Description).IsRequired().HasMaxLength(4000);
        builder.Property(j => j.Location).HasMaxLength(200);
        builder.Property(j => j.EmploymentType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(j => j.SalaryMin).HasPrecision(12, 2);
        builder.Property(j => j.SalaryMax).HasPrecision(12, 2);
        builder.Property(j => j.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(j => j.CreatedAt).IsRequired();
        builder.Property(j => j.UpdatedAt).IsRequired();

        builder.HasIndex(j => j.BusinessPartnerProfileId);
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.EmploymentType);

        builder.HasOne(j => j.BusinessPartnerProfile)
            .WithMany()
            .HasForeignKey(j => j.BusinessPartnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(j => j.SkillRequirements)
            .WithOne(r => r.JobListing)
            .HasForeignKey(r => r.JobListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(j => j.Applications)
            .WithOne(a => a.JobListing)
            .HasForeignKey(a => a.JobListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
