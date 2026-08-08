using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerHeritageIdentityConfiguration : IEntityTypeConfiguration<ProducerHeritageIdentity>
{
    public void Configure(EntityTypeBuilder<ProducerHeritageIdentity> builder)
    {
        builder.ToTable("ProducerHeritageIdentities");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.HeritageIdNumber).IsRequired().HasMaxLength(50);
        builder.Property(h => h.PrimaryCraft).IsRequired().HasMaxLength(100);
        builder.Property(h => h.WorkshopName).IsRequired().HasMaxLength(200);
        builder.Property(h => h.WorkshopDescription).IsRequired().HasMaxLength(2000);
        builder.Property(h => h.WorkshopAddress).HasMaxLength(300);
        builder.Property(h => h.VerificationNotes).HasMaxLength(1000);
        builder.Property(h => h.VerificationStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.CreatedAt).IsRequired();
        builder.Property(h => h.UpdatedAt).IsRequired();

        builder.HasIndex(h => h.ProducerId).IsUnique();
        builder.HasIndex(h => h.HeritageIdNumber).IsUnique();

        builder.HasOne(h => h.Producer)
            .WithMany()
            .HasForeignKey(h => h.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.VerifiedBy)
            .WithMany()
            .HasForeignKey(h => h.VerifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.District)
            .WithMany()
            .HasForeignKey(h => h.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(h => h.FamilyMembers)
            .WithOne(m => m.ProducerHeritageIdentity)
            .HasForeignKey(m => m.ProducerHeritageIdentityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.SkillTimeline)
            .WithOne(s => s.ProducerHeritageIdentity)
            .HasForeignKey(s => s.ProducerHeritageIdentityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.Awards)
            .WithOne(a => a.ProducerHeritageIdentity)
            .HasForeignKey(a => a.ProducerHeritageIdentityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.Certifications)
            .WithOne(c => c.ProducerHeritageIdentity)
            .HasForeignKey(c => c.ProducerHeritageIdentityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.StoryArchive)
            .WithOne(s => s.ProducerHeritageIdentity)
            .HasForeignKey(s => s.ProducerHeritageIdentityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.ScoreHistory)
            .WithOne(e => e.ProducerHeritageIdentity)
            .HasForeignKey(e => e.ProducerHeritageIdentityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
