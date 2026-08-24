using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Data.Configurations;

public class VillageTourStopConfiguration : IEntityTypeConfiguration<VillageTourStop>
{
    public void Configure(EntityTypeBuilder<VillageTourStop> builder)
    {
        builder.ToTable("VillageTourStops");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Description).HasMaxLength(2000);
        builder.Property(s => s.MediaUrl).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.MediaType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ThumbnailUrl).HasMaxLength(1000);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.HeritagePlaceId);
        builder.HasIndex(s => s.IsActive);

        builder.HasOne(s => s.HeritagePlace)
            .WithMany()
            .HasForeignKey(s => s.HeritagePlaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
