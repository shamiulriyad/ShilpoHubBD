using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Mentorship;

namespace ShilpoHubBD.Data.Configurations;

public class MentorshipRequestConfiguration : IEntityTypeConfiguration<MentorshipRequest>
{
    public void Configure(EntityTypeBuilder<MentorshipRequest> builder)
    {
        builder.ToTable("MentorshipRequests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Message).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.ResponseMessage).HasMaxLength(2000);
        builder.Property(r => r.RequestedAt).IsRequired();

        builder.HasIndex(r => new { r.MentorProfileId, r.Status });
        builder.HasIndex(r => new { r.LearnerUserId, r.Status });

        builder.HasOne(r => r.MentorProfile)
            .WithMany()
            .HasForeignKey(r => r.MentorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Learner)
            .WithMany()
            .HasForeignKey(r => r.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.HeritageSkill)
            .WithMany()
            .HasForeignKey(r => r.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
