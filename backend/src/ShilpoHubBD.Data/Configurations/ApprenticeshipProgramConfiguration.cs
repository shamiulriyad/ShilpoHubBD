using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Data.Configurations;

public class ApprenticeshipProgramConfiguration : IEntityTypeConfiguration<ApprenticeshipProgram>
{
    public void Configure(EntityTypeBuilder<ApprenticeshipProgram> builder)
    {
        builder.ToTable("ApprenticeshipPrograms");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.Location).HasMaxLength(200);
        builder.Property(p => p.EligibilityRequirements).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.MentorId);
        builder.HasIndex(p => p.TrainerProfileId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.HeritageSkillId);

        builder.HasOne(p => p.Mentor)
            .WithMany()
            .HasForeignKey(p => p.MentorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.TrainerProfile)
            .WithMany()
            .HasForeignKey(p => p.TrainerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.HeritageSkill)
            .WithMany()
            .HasForeignKey(p => p.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Milestones)
            .WithOne(m => m.Program)
            .HasForeignKey(m => m.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Applications)
            .WithOne(a => a.Program)
            .HasForeignKey(a => a.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Enrollments)
            .WithOne(e => e.Program)
            .HasForeignKey(e => e.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
