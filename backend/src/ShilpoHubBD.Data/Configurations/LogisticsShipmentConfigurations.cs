using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TrackingNumber).IsRequired().HasMaxLength(40);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ServiceLevel).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.Property(s => s.OriginContactName).IsRequired().HasMaxLength(160);
        builder.Property(s => s.OriginPhone).IsRequired().HasMaxLength(40);
        builder.Property(s => s.OriginAddressLine).IsRequired().HasMaxLength(400);
        builder.Property(s => s.OriginCity).IsRequired().HasMaxLength(120);
        builder.Property(s => s.OriginPostalCode).HasMaxLength(20);

        builder.Property(s => s.RecipientName).IsRequired().HasMaxLength(160);
        builder.Property(s => s.RecipientPhone).IsRequired().HasMaxLength(40);
        builder.Property(s => s.DestinationAddressLine).IsRequired().HasMaxLength(400);
        builder.Property(s => s.DestinationCity).IsRequired().HasMaxLength(120);
        builder.Property(s => s.DestinationPostalCode).HasMaxLength(20);

        builder.Property(s => s.DimensionsNote).HasMaxLength(400);
        builder.Property(s => s.TotalWeightKg).HasColumnType("numeric(12,2)");
        builder.Property(s => s.DeclaredValue).HasColumnType("numeric(14,2)");
        builder.Property(s => s.ShippingCost).HasColumnType("numeric(14,2)");
        builder.Property(s => s.CodAmount).HasColumnType("numeric(14,2)");

        builder.Property(s => s.CurrentLocationLabel).HasMaxLength(200);
        builder.Property(s => s.ReceivedByName).HasMaxLength(160);
        builder.Property(s => s.ProofOfDeliveryNote).HasMaxLength(2000);
        builder.Property(s => s.SignatureImageUrl).HasMaxLength(1000);
        builder.Property(s => s.FailureReason).HasMaxLength(1000);
        builder.Property(s => s.CancellationReason).HasMaxLength(1000);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.TrackingNumber).IsUnique();
        builder.HasIndex(s => new { s.LogisticsPartnerProfileId, s.Status });
        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.PickupRequestId);
        builder.HasIndex(s => s.DeliveryRouteId);
        builder.HasIndex(s => s.EstimatedDeliveryAt);

        builder.HasOne(s => s.Profile)
            .WithMany()
            .HasForeignKey(s => s.LogisticsPartnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.CreatedBy)
            .WithMany()
            .HasForeignKey(s => s.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Order)
            .WithMany()
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.PickupRequest)
            .WithMany()
            .HasForeignKey(s => s.PickupRequestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.DeliveryRoute)
            .WithMany()
            .HasForeignKey(s => s.DeliveryRouteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.OriginDistrict)
            .WithMany()
            .HasForeignKey(s => s.OriginDistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.DestinationDistrict)
            .WithMany()
            .HasForeignKey(s => s.DestinationDistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Events)
            .WithOne(e => e.Shipment)
            .HasForeignKey(e => e.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Attempts)
            .WithOne(a => a.Shipment)
            .HasForeignKey(a => a.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ShipmentTrackingEventConfiguration : IEntityTypeConfiguration<ShipmentTrackingEvent>
{
    public void Configure(EntityTypeBuilder<ShipmentTrackingEvent> builder)
    {
        builder.ToTable("ShipmentTrackingEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.LocationLabel).HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.OccurredAt).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => new { e.ShipmentId, e.OccurredAt });

        builder.HasOne(e => e.District)
            .WithMany()
            .HasForeignKey(e => e.DistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.RecordedBy)
            .WithMany()
            .HasForeignKey(e => e.RecordedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("DeliveryAttempts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Outcome).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.AttemptedAt).IsRequired();
        builder.Property(a => a.Note).HasMaxLength(2000);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => new { a.ShipmentId, a.AttemptNumber });

        builder.HasOne(a => a.RecordedBy)
            .WithMany()
            .HasForeignKey(a => a.RecordedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
