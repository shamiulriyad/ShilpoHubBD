using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Portfolio;

namespace ShilpoHubBD.Data.Configurations;

public class MentorFeedbackConfiguration : IEntityTypeConfiguration<MentorFeedback>
{
    public void Configure(EntityTypeBuilder<MentorFeedback> builder)
    {
        builder.ToTable("MentorFeedbacks");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Message).IsRequired().HasMaxLength(2000);
        builder.Property(f => f.CreatedAt).IsRequired();

        builder.HasIndex(f => f.LearnerUserId);
        builder.HasIndex(f => f.MentorProfileId);

        builder.HasOne(f => f.MentorProfile)
            .WithMany()
            .HasForeignKey(f => f.MentorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Learner)
            .WithMany()
            .HasForeignKey(f => f.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.HeritageSkill)
            .WithMany()
            .HasForeignKey(f => f.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
