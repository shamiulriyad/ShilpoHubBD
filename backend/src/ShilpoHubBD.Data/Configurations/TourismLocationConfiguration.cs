using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Data.Configurations;

public class TourismLocationConfiguration : IEntityTypeConfiguration<TourismLocation>
{
    public void Configure(EntityTypeBuilder<TourismLocation> builder)
    {
        builder.ToTable("TourismLocations");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(l => l.Description).IsRequired().HasMaxLength(4000);
        builder.Property(l => l.Address).HasMaxLength(500);
        builder.Property(l => l.Latitude).IsRequired();
        builder.Property(l => l.Longitude).IsRequired();
        builder.Property(l => l.Price).HasColumnType("decimal(18,2)");
        builder.Property(l => l.EntryFee).HasColumnType("decimal(18,2)");
        builder.Property(l => l.OpeningHours).HasMaxLength(200);
        builder.Property(l => l.ContactInfo).HasMaxLength(300);
        builder.Property(l => l.Facilities).HasMaxLength(1000);
        builder.Property(l => l.ImageUrl).HasMaxLength(1000);
        builder.Property(l => l.ImageCredit).HasMaxLength(300);
        builder.Property(l => l.ImageSourceUrl).HasMaxLength(1000);
        builder.Property(l => l.Area).HasMaxLength(200);
        builder.Property(l => l.SourceUrl).HasMaxLength(500);
        builder.Property(l => l.VerificationStatus).IsRequired().HasMaxLength(20).HasDefaultValue("Unverified");
        builder.Property(l => l.UnverifiedFields).HasMaxLength(1000);
        builder.Property(l => l.CoordinatesSource).HasMaxLength(300);
        builder.Property(l => l.CoordinatesPrecision).HasMaxLength(30);
        builder.Property(l => l.Source).IsRequired().HasMaxLength(30).HasDefaultValue("Admin");
        builder.Property(l => l.ExternalId).HasMaxLength(100);
        builder.Property(l => l.Upazila).HasMaxLength(200);
        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.UpdatedAt).IsRequired();

        builder.HasIndex(l => new { l.Latitude, l.Longitude });
        builder.HasIndex(l => l.Type);
        builder.HasIndex(l => l.IsActive);
        builder.HasIndex(l => l.DistrictId);
        builder.HasIndex(l => new { l.Source, l.ExternalId }).IsUnique().HasFilter("\"ExternalId\" IS NOT NULL");

        builder.HasOne(l => l.District)
            .WithMany()
            .HasForeignKey(l => l.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
