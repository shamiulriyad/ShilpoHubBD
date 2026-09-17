using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Configurations;

public class HomepageSectionConfiguration : IEntityTypeConfiguration<HomepageSection>
{
    public void Configure(EntityTypeBuilder<HomepageSection> builder)
    {
        builder.ToTable("HomepageSections");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SectionKey).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Subtitle).HasMaxLength(500);
        builder.Property(s => s.ImageUrl).HasMaxLength(1000);
        builder.Property(s => s.LinkUrl).HasMaxLength(1000);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.SectionKey).IsUnique();
        builder.HasIndex(s => new { s.IsActive, s.DisplayOrder });
    }
}
