using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerStoryChapterConfiguration : IEntityTypeConfiguration<ProducerStoryChapter>
{
    public void Configure(EntityTypeBuilder<ProducerStoryChapter> builder)
    {
        builder.ToTable("ProducerStoryChapters");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Heading).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Body).IsRequired().HasMaxLength(4000);
        builder.Property(c => c.DisplayOrder).IsRequired();

        builder.HasIndex(c => c.ProducerStoryId);
    }
}
