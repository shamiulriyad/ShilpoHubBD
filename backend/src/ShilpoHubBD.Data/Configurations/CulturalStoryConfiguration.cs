using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Data.Configurations;

public class CulturalStoryConfiguration : IEntityTypeConfiguration<CulturalStory>
{
    public void Configure(EntityTypeBuilder<CulturalStory> builder)
    {
        builder.ToTable("CulturalStories");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Summary).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.CoverImageUrl).HasMaxLength(1000);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.HeritagePlaceId);

        builder.HasOne(s => s.HeritagePlace)
            .WithMany()
            .HasForeignKey(s => s.HeritagePlaceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Chapters)
            .WithOne(c => c.CulturalStory)
            .HasForeignKey(c => c.CulturalStoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
