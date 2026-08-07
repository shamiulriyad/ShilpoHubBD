using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerStoryConfiguration : IEntityTypeConfiguration<ProducerStory>
{
    public void Configure(EntityTypeBuilder<ProducerStory> builder)
    {
        builder.ToTable("ProducerStories");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.HeritageId).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.HeritageId).IsUnique();

        builder.Property(s => s.Generations).IsRequired();
        builder.Property(s => s.Quote).IsRequired().HasMaxLength(500);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.ProducerId).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Chapters)
            .WithOne(c => c.ProducerStory)
            .HasForeignKey(c => c.ProducerStoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
