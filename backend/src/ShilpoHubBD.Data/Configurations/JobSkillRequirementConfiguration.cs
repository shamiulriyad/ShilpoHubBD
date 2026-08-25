using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Employment;

namespace ShilpoHubBD.Data.Configurations;

public class JobSkillRequirementConfiguration : IEntityTypeConfiguration<JobSkillRequirement>
{
    public void Configure(EntityTypeBuilder<JobSkillRequirement> builder)
    {
        builder.ToTable("JobSkillRequirements");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.MinLevel).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(r => new { r.JobListingId, r.HeritageSkillId }).IsUnique();

        builder.HasOne(r => r.HeritageSkill)
            .WithMany()
            .HasForeignKey(r => r.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
