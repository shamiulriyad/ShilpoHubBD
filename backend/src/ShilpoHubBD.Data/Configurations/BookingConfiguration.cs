using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.TotalPrice).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(b => b.Notes).HasMaxLength(2000);
        builder.Property(b => b.CancellationReason).HasMaxLength(1000);
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.TouristId);
        builder.HasIndex(b => b.ProducerId);
        builder.HasIndex(b => b.AvailabilitySlotId);

        builder.HasOne(b => b.Tourist)
            .WithMany()
            .HasForeignKey(b => b.TouristId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Producer)
            .WithMany()
            .HasForeignKey(b => b.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
