using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Data.Configurations;

public class MuseumItemMediaConfiguration : IEntityTypeConfiguration<MuseumItemMedia>
{
    public void Configure(EntityTypeBuilder<MuseumItemMedia> builder)
    {
        builder.ToTable("MuseumItemMedia");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MediaUrl).IsRequired().HasMaxLength(1000);
        builder.Property(m => m.MediaType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Caption).HasMaxLength(300);
    }
}
