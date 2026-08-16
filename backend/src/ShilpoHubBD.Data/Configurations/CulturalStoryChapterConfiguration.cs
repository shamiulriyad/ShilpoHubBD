using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Data.Configurations;

public class CulturalStoryChapterConfiguration : IEntityTypeConfiguration<CulturalStoryChapter>
{
    public void Configure(EntityTypeBuilder<CulturalStoryChapter> builder)
    {
        builder.ToTable("CulturalStoryChapters");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Heading).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Body).IsRequired().HasMaxLength(4000);
        builder.Property(c => c.MediaUrl).HasMaxLength(1000);
        builder.Property(c => c.MediaType).HasConversion<string>().HasMaxLength(20);
    }
}
