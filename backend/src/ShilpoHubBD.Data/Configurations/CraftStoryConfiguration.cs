using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Configurations;

public class CraftStoryConfiguration : IEntityTypeConfiguration<CraftStory>
{
    public void Configure(EntityTypeBuilder<CraftStory> builder)
    {
        builder.ToTable("CraftStories");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Origin).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Since).IsRequired();
        builder.Property(s => s.Summary).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.CategoryId).IsUnique();

        builder.HasOne(s => s.Category)
            .WithMany()
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Chapters)
            .WithOne(c => c.CraftStory)
            .HasForeignKey(c => c.CraftStoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
