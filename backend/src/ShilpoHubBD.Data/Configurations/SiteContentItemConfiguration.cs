using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Configurations;

public class SiteContentItemConfiguration : IEntityTypeConfiguration<SiteContentItem>
{
    public void Configure(EntityTypeBuilder<SiteContentItem> builder)
    {
        builder.ToTable("SiteContentItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Group).IsRequired().HasMaxLength(60);
        builder.Property(i => i.Title).IsRequired().HasMaxLength(300);
        builder.Property(i => i.Subtitle).HasMaxLength(300);
        builder.Property(i => i.Body).HasMaxLength(4000);
        builder.Property(i => i.LinkUrl).HasMaxLength(1000);
        builder.Property(i => i.Extra).HasMaxLength(4000);
        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.UpdatedAt).IsRequired();

        builder.HasIndex(i => new { i.Group, i.DisplayOrder });
        builder.HasIndex(i => i.IsActive);
    }
}
