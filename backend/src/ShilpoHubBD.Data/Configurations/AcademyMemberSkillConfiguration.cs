using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Configurations;

public class AcademyMemberSkillConfiguration : IEntityTypeConfiguration<AcademyMemberSkill>
{
    public void Configure(EntityTypeBuilder<AcademyMemberSkill> builder)
    {
        builder.ToTable("AcademyMemberSkills");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Level).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.AddedAt).IsRequired();

        builder.HasIndex(s => new { s.AcademyMemberProfileId, s.HeritageSkillId }).IsUnique();

        builder.HasOne(s => s.AcademyMemberProfile)
            .WithMany(p => p.Skills)
            .HasForeignKey(s => s.AcademyMemberProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.HeritageSkill)
            .WithMany()
            .HasForeignKey(s => s.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
