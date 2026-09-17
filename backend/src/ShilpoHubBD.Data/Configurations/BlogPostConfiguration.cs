using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Slug).IsRequired().HasMaxLength(220);
        builder.Property(b => b.Summary).IsRequired().HasMaxLength(500);
        builder.Property(b => b.Content).IsRequired().HasMaxLength(20000);
        builder.Property(b => b.CoverImageUrl).HasMaxLength(1000);
        builder.Property(b => b.Tags).HasMaxLength(500);
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.HasIndex(b => b.Slug).IsUnique();
        builder.HasIndex(b => new { b.IsPublished, b.PublishedAt });

        builder.HasOne(b => b.Author)
            .WithMany()
            .HasForeignKey(b => b.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
