using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.LegalName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Phone).IsRequired().HasMaxLength(30);
        builder.Property(p => p.NidNumber).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Expertise).HasMaxLength(200);
        builder.Property(p => p.AddressLine).IsRequired().HasMaxLength(500);
        builder.Property(p => p.About).HasMaxLength(2000);
        builder.Property(p => p.ReviewNotes).HasMaxLength(2000);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.NidNumber).IsUnique();
        builder.HasIndex(p => p.Status);

        builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.ReviewedBy).WithMany().HasForeignKey(p => p.ReviewedByUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(p => p.District).WithMany().HasForeignKey(p => p.DistrictId).OnDelete(DeleteBehavior.SetNull);
    }
}
