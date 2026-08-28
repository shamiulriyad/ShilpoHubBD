using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Configurations;

public class AcademyMemberProfileConfiguration : IEntityTypeConfiguration<AcademyMemberProfile>
{
    public void Configure(EntityTypeBuilder<AcademyMemberProfile> builder)
    {
        builder.ToTable("AcademyMemberProfiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Bio).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.LearningPreferences).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.UserId).IsUnique();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
