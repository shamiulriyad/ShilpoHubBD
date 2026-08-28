using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Configurations;

public class MentorProfileConfiguration : IEntityTypeConfiguration<MentorProfile>
{
    public void Configure(EntityTypeBuilder<MentorProfile> builder)
    {
        builder.ToTable("MentorProfiles");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Bio).IsRequired().HasMaxLength(2000);
        builder.Property(m => m.Expertise).IsRequired().HasMaxLength(500);
        builder.Property(m => m.YearsOfExperience).IsRequired();
        builder.Property(m => m.IsActive).IsRequired();
        builder.Property(m => m.Location).HasMaxLength(200);
        builder.Property(m => m.AvailabilityNote).HasMaxLength(500);
        builder.Property(m => m.PreferredCategory).HasMaxLength(100);
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();

        builder.HasIndex(m => m.UserId).IsUnique();

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Skills)
            .WithOne(s => s.MentorProfile)
            .HasForeignKey(s => s.MentorProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
