using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Configurations;

public class NewsItemConfiguration : IEntityTypeConfiguration<NewsItem>
{
    public void Configure(EntityTypeBuilder<NewsItem> builder)
    {
        builder.ToTable("NewsItems");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Slug).IsRequired().HasMaxLength(220);
        builder.Property(n => n.Summary).IsRequired().HasMaxLength(500);
        builder.Property(n => n.Content).IsRequired().HasMaxLength(20000);
        builder.Property(n => n.ImageUrl).HasMaxLength(1000);
        builder.Property(n => n.Source).HasMaxLength(200);
        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.UpdatedAt).IsRequired();

        builder.HasIndex(n => n.Slug).IsUnique();
        builder.HasIndex(n => new { n.IsPublished, n.PublishedAt });
    }
}
