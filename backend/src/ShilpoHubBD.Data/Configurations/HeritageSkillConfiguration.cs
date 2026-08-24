using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Configurations;

public class HeritageSkillConfiguration : IEntityTypeConfiguration<HeritageSkill>
{
    public void Configure(EntityTypeBuilder<HeritageSkill> builder)
    {
        builder.ToTable("HeritageSkills");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Description).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasIndex(s => s.Name).IsUnique();
    }
}
