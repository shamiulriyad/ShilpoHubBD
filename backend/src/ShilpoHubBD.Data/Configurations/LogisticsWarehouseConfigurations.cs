using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Code).IsRequired().HasMaxLength(40);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(w => w.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(w => w.AddressLine).IsRequired().HasMaxLength(400);
        builder.Property(w => w.City).IsRequired().HasMaxLength(120);
        builder.Property(w => w.PostalCode).HasMaxLength(20);
        builder.Property(w => w.ContactPersonName).HasMaxLength(160);
        builder.Property(w => w.ContactPhone).HasMaxLength(40);
        builder.Property(w => w.Notes).HasMaxLength(2000);
        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.UpdatedAt).IsRequired();

        builder.HasIndex(w => w.Code).IsUnique();
        builder.HasIndex(w => new { w.LogisticsPartnerProfileId, w.Status });

        builder.HasOne(w => w.Profile)
            .WithMany()
            .HasForeignKey(w => w.LogisticsPartnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.CreatedBy)
            .WithMany()
            .HasForeignKey(w => w.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.District)
            .WithMany()
            .HasForeignKey(w => w.DistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(w => w.Zones)
            .WithOne(z => z.Warehouse)
            .HasForeignKey(z => z.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Bins)
            .WithOne(b => b.Warehouse)
            .HasForeignKey(b => b.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WarehouseZoneConfiguration : IEntityTypeConfiguration<WarehouseZone>
{
    public void Configure(EntityTypeBuilder<WarehouseZone> builder)
    {
        builder.ToTable("WarehouseZones");
        builder.HasKey(z => z.Id);

        builder.Property(z => z.Code).IsRequired().HasMaxLength(40);
        builder.Property(z => z.Name).IsRequired().HasMaxLength(160);
        builder.Property(z => z.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(z => z.Notes).HasMaxLength(1000);
        builder.Property(z => z.CreatedAt).IsRequired();
        builder.Property(z => z.UpdatedAt).IsRequired();

        builder.HasIndex(z => new { z.WarehouseId, z.Code }).IsUnique();
    }
}

public class WarehouseBinConfiguration : IEntityTypeConfiguration<WarehouseBin>
{
    public void Configure(EntityTypeBuilder<WarehouseBin> builder)
    {
        builder.ToTable("WarehouseBins");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Code).IsRequired().HasMaxLength(60);
        builder.Property(b => b.Label).HasMaxLength(160);
        builder.Property(b => b.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.HasIndex(b => new { b.WarehouseId, b.Code }).IsUnique();
        builder.HasIndex(b => b.WarehouseZoneId);

        builder.HasOne(b => b.Zone)
            .WithMany()
            .HasForeignKey(b => b.WarehouseZoneId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class WarehouseStockItemConfiguration : IEntityTypeConfiguration<WarehouseStockItem>
{
    public void Configure(EntityTypeBuilder<WarehouseStockItem> builder)
    {
        builder.ToTable("WarehouseStockItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku).IsRequired().HasMaxLength(80);
        builder.Property(i => i.Description).IsRequired().HasMaxLength(400);
        builder.Property(i => i.UnitOfMeasure).IsRequired().HasMaxLength(20);
        builder.Property(i => i.BatchNumber).HasMaxLength(80);
        builder.Property(i => i.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.UnitValue).HasColumnType("numeric(14,2)");
        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.UpdatedAt).IsRequired();

        builder.HasIndex(i => new { i.WarehouseId, i.Sku });
        builder.HasIndex(i => i.WarehouseBinId);
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.OwnerUserId);
        builder.HasIndex(i => i.ExpiryDate);

        builder.HasOne(i => i.Warehouse)
            .WithMany()
            .HasForeignKey(i => i.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Bin)
            .WithMany()
            .HasForeignKey(i => i.WarehouseBinId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Owner)
            .WithMany()
            .HasForeignKey(i => i.OwnerUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class WarehouseStockMovementConfiguration : IEntityTypeConfiguration<WarehouseStockMovement>
{
    public void Configure(EntityTypeBuilder<WarehouseStockMovement> builder)
    {
        builder.ToTable("WarehouseStockMovements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Sku).IsRequired().HasMaxLength(80);
        builder.Property(m => m.ReferenceType).HasMaxLength(40);
        builder.Property(m => m.Reason).HasMaxLength(400);
        builder.Property(m => m.Note).HasMaxLength(2000);
        builder.Property(m => m.OccurredAt).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();

        builder.HasIndex(m => new { m.WarehouseId, m.OccurredAt });
        builder.HasIndex(m => m.WarehouseStockItemId);
        builder.HasIndex(m => new { m.ReferenceType, m.ReferenceId });

        builder.HasOne(m => m.Warehouse)
            .WithMany()
            .HasForeignKey(m => m.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.StockItem)
            .WithMany()
            .HasForeignKey(m => m.WarehouseStockItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.PerformedBy)
            .WithMany()
            .HasForeignKey(m => m.PerformedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
