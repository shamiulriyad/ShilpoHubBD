using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Configurations;

public class TouristServiceConfiguration : IEntityTypeConfiguration<TouristService>
{
    public void Configure(EntityTypeBuilder<TouristService> builder)
    {
        builder.ToTable("TouristServices");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Description).IsRequired().HasMaxLength(4000);
        builder.Property(s => s.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.Price).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(s => s.Location).HasMaxLength(300);
        builder.Property(s => s.ImageUrl).HasMaxLength(1000);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.Type);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.ProducerId);

        builder.HasOne(s => s.Producer)
            .WithMany()
            .HasForeignKey(s => s.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.District)
            .WithMany()
            .HasForeignKey(s => s.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.AvailabilitySlots)
            .WithOne(a => a.Service)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Bookings)
            .WithOne(b => b.Service)
            .HasForeignKey(b => b.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
