using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Configurations;

public class DeliveryRouteConfiguration : IEntityTypeConfiguration<DeliveryRoute>
{
    public void Configure(EntityTypeBuilder<DeliveryRoute> builder)
    {
        builder.ToTable("DeliveryRoutes");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RouteCode).IsRequired().HasMaxLength(40);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.StartLocationLabel).HasMaxLength(200);
        builder.Property(r => r.EndLocationLabel).HasMaxLength(200);
        builder.Property(r => r.AssignedDriverName).HasMaxLength(160);
        builder.Property(r => r.AssignedDriverPhone).HasMaxLength(40);
        builder.Property(r => r.AssignedVehicleLabel).HasMaxLength(80);
        builder.Property(r => r.VehicleCapacityKg).HasColumnType("numeric(12,2)");
        builder.Property(r => r.TotalLoadKg).HasColumnType("numeric(12,2)");
        builder.Property(r => r.TotalDistanceKm).HasColumnType("numeric(10,2)");
        builder.Property(r => r.OptimizationStrategy).IsRequired().HasMaxLength(40);
        builder.Property(r => r.Notes).HasMaxLength(2000);
        builder.Property(r => r.CancellationReason).HasMaxLength(1000);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.RouteCode).IsUnique();
        builder.HasIndex(r => new { r.LogisticsPartnerProfileId, r.Status });
        builder.HasIndex(r => r.ScheduledDate);

        builder.HasOne(r => r.Profile)
            .WithMany()
            .HasForeignKey(r => r.LogisticsPartnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.OriginDistrict)
            .WithMany()
            .HasForeignKey(r => r.OriginDistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Stops)
            .WithOne(s => s.Route)
            .HasForeignKey(s => s.DeliveryRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Events)
            .WithOne(e => e.Route)
            .HasForeignKey(e => e.DeliveryRouteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DeliveryRouteStopConfiguration : IEntityTypeConfiguration<DeliveryRouteStop>
{
    public void Configure(EntityTypeBuilder<DeliveryRouteStop> builder)
    {
        builder.ToTable("DeliveryRouteStops");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.StopType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ContactName).HasMaxLength(160);
        builder.Property(s => s.ContactPhone).HasMaxLength(40);
        builder.Property(s => s.AddressLine).IsRequired().HasMaxLength(400);
        builder.Property(s => s.City).IsRequired().HasMaxLength(120);
        builder.Property(s => s.PostalCode).HasMaxLength(20);
        builder.Property(s => s.LoadKg).HasColumnType("numeric(12,2)");
        builder.Property(s => s.DistanceFromPreviousKm).HasColumnType("numeric(10,2)");
        builder.Property(s => s.Instructions).HasMaxLength(2000);
        builder.Property(s => s.CompletionNote).HasMaxLength(2000);
        builder.Property(s => s.FailureReason).HasMaxLength(1000);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => new { s.DeliveryRouteId, s.Sequence });
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.PickupRequestId);
        builder.HasIndex(s => s.OrderId);

        builder.HasOne(s => s.PickupRequest)
            .WithMany()
            .HasForeignKey(s => s.PickupRequestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.Order)
            .WithMany()
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.District)
            .WithMany()
            .HasForeignKey(s => s.DistrictId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class DeliveryRouteEventConfiguration : IEntityTypeConfiguration<DeliveryRouteEvent>
{
    public void Configure(EntityTypeBuilder<DeliveryRouteEvent> builder)
    {
        builder.ToTable("DeliveryRouteEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => new { e.DeliveryRouteId, e.CreatedAt });

        builder.HasOne(e => e.Actor)
            .WithMany()
            .HasForeignKey(e => e.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
