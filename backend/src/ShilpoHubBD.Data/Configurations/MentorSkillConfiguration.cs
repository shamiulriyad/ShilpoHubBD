using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Configurations;

public class MentorSkillConfiguration : IEntityTypeConfiguration<MentorSkill>
{
    public void Configure(EntityTypeBuilder<MentorSkill> builder)
    {
        builder.ToTable("MentorSkills");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Level).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.AddedAt).IsRequired();

        builder.HasIndex(s => new { s.MentorProfileId, s.HeritageSkillId }).IsUnique();

        builder.HasOne(s => s.HeritageSkill)
            .WithMany()
            .HasForeignKey(s => s.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
