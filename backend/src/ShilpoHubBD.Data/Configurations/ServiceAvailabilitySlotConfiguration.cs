using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Configurations;

public class ServiceAvailabilitySlotConfiguration : IEntityTypeConfiguration<ServiceAvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<ServiceAvailabilitySlot> builder)
    {
        builder.ToTable("ServiceAvailabilitySlots");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.StartAt).IsRequired();
        builder.Property(a => a.EndAt).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        builder.HasIndex(a => a.ServiceId);
        builder.HasIndex(a => a.StartAt);
        builder.HasIndex(a => a.IsActive);

        builder.HasMany(a => a.Bookings)
            .WithOne(b => b.AvailabilitySlot)
            .HasForeignKey(b => b.AvailabilitySlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
