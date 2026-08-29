using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Configurations;

public class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
{
    public void Configure(EntityTypeBuilder<ReturnRequest> builder)
    {
        builder.ToTable("ReturnRequests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReferenceCode).IsRequired().HasMaxLength(40);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Reason).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.ReasonDetail).HasMaxLength(2000);
        builder.Property(r => r.CustomerName).IsRequired().HasMaxLength(160);
        builder.Property(r => r.CustomerPhone).IsRequired().HasMaxLength(40);

        builder.Property(r => r.PickupContactName).HasMaxLength(160);
        builder.Property(r => r.PickupPhone).HasMaxLength(40);
        builder.Property(r => r.PickupAddressLine).HasMaxLength(400);
        builder.Property(r => r.PickupCity).HasMaxLength(120);
        builder.Property(r => r.PickupPostalCode).HasMaxLength(20);
        builder.Property(r => r.AssignedCarrierLabel).HasMaxLength(80);
        builder.Property(r => r.AssignedDriverName).HasMaxLength(160);

        builder.Property(r => r.ResolutionType).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.ResolutionNote).HasMaxLength(2000);
        builder.Property(r => r.RefundAmount).HasColumnType("numeric(14,2)");
        builder.Property(r => r.RefundMethod).HasMaxLength(40);
        builder.Property(r => r.RefundReference).HasMaxLength(120);
        builder.Property(r => r.RejectionReason).HasMaxLength(1000);
        builder.Property(r => r.CancellationReason).HasMaxLength(1000);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.ReferenceCode).IsUnique();
        builder.HasIndex(r => new { r.LogisticsPartnerProfileId, r.Status });
        builder.HasIndex(r => r.ShipmentId);
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => r.DestinationWarehouseId);

        builder.HasOne(r => r.Profile)
            .WithMany()
            .HasForeignKey(r => r.LogisticsPartnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Shipment)
            .WithMany()
            .HasForeignKey(r => r.ShipmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(r => r.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.PickupDistrict)
            .WithMany()
            .HasForeignKey(r => r.PickupDistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.ApprovedBy)
            .WithMany()
            .HasForeignKey(r => r.ApprovedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Items)
            .WithOne(i => i.ReturnRequest)
            .HasForeignKey(i => i.ReturnRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Inspections)
            .WithOne(i => i.ReturnRequest)
            .HasForeignKey(i => i.ReturnRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Events)
            .WithOne(e => e.ReturnRequest)
            .HasForeignKey(e => e.ReturnRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReturnItemConfiguration : IEntityTypeConfiguration<ReturnItem>
{
    public void Configure(EntityTypeBuilder<ReturnItem> builder)
    {
        builder.ToTable("ReturnItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku).HasMaxLength(80);
        builder.Property(i => i.Description).IsRequired().HasMaxLength(400);
        builder.Property(i => i.Condition).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Disposition).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.UnitRefundAmount).HasColumnType("numeric(14,2)");
        builder.Property(i => i.Notes).HasMaxLength(1000);

        builder.HasIndex(i => i.ReturnRequestId);
        builder.HasIndex(i => i.ProductId);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ReturnInspectionConfiguration : IEntityTypeConfiguration<ReturnInspection>
{
    public void Configure(EntityTypeBuilder<ReturnInspection> builder)
    {
        builder.ToTable("ReturnInspections");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InspectedAt).IsRequired();
        builder.Property(i => i.OverallCondition).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(i => i.RecommendedResolution).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.PhotosJson).HasColumnType("text");
        builder.Property(i => i.CreatedAt).IsRequired();

        builder.HasIndex(i => new { i.ReturnRequestId, i.InspectedAt });

        builder.HasOne(i => i.InspectedBy)
            .WithMany()
            .HasForeignKey(i => i.InspectedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ReturnEventConfiguration : IEntityTypeConfiguration<ReturnEvent>
{
    public void Configure(EntityTypeBuilder<ReturnEvent> builder)
    {
        builder.ToTable("ReturnEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => new { e.ReturnRequestId, e.CreatedAt });

        builder.HasOne(e => e.Actor)
            .WithMany()
            .HasForeignKey(e => e.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
