using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Configurations;

public class CmsEventConfiguration : IEntityTypeConfiguration<CmsEvent>
{
    public void Configure(EntityTypeBuilder<CmsEvent> builder)
    {
        builder.ToTable("CmsEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Slug).IsRequired().HasMaxLength(220);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.ImageUrl).HasMaxLength(1000);
        builder.Property(e => e.Location).HasMaxLength(300);
        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.EndDate).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => new { e.IsPublished, e.StartDate });
    }
}
