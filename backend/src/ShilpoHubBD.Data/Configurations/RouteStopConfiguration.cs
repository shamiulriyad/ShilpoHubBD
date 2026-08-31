using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Configurations;

public class RouteStopConfiguration : IEntityTypeConfiguration<RouteStop>
{
    public void Configure(EntityTypeBuilder<RouteStop> builder)
    {
        builder.ToTable("RouteStops");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Order).IsRequired();
        builder.Property(s => s.TransportationMode).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Notes).HasMaxLength(1000);

        builder.HasIndex(s => new { s.RouteId, s.Order }).IsUnique();
        builder.HasIndex(s => new { s.RouteId, s.HeritagePlaceId }).IsUnique();

        builder.HasOne(s => s.HeritagePlace)
            .WithMany()
            .HasForeignKey(s => s.HeritagePlaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
