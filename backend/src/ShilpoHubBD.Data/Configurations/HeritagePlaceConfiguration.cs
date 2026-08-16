using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Configurations;

public class HeritagePlaceConfiguration : IEntityTypeConfiguration<HeritagePlace>
{
    public void Configure(EntityTypeBuilder<HeritagePlace> builder)
    {
        builder.ToTable("HeritagePlaces");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.PlaceType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Address).HasMaxLength(500);
        builder.Property(p => p.ImageUrl).HasMaxLength(1000);
        builder.Property(p => p.Latitude).IsRequired();
        builder.Property(p => p.Longitude).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => new { p.Latitude, p.Longitude });
        builder.HasIndex(p => p.PlaceType);
        builder.HasIndex(p => p.IsActive);

        builder.HasOne(p => p.District)
            .WithMany()
            .HasForeignKey(p => p.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Festivals)
            .WithOne(f => f.HeritagePlace)
            .HasForeignKey(f => f.HeritagePlaceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Events)
            .WithOne(e => e.HeritagePlace)
            .HasForeignKey(e => e.HeritagePlaceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.CuisineItems)
            .WithOne(c => c.HeritagePlace)
            .HasForeignKey(c => c.HeritagePlaceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
