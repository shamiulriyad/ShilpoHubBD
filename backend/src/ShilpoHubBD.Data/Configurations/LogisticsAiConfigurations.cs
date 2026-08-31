using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Configurations;

public class DeliveryPredictionConfiguration : IEntityTypeConfiguration<DeliveryPrediction>
{
    public void Configure(EntityTypeBuilder<DeliveryPrediction> builder)
    {
        builder.ToTable("DeliveryPredictions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Method).IsRequired().HasMaxLength(60);
        builder.Property(p => p.RiskLevel).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Confidence).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.FactorsJson).HasColumnType("text");
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => new { p.LogisticsPartnerProfileId, p.CreatedAt });
        builder.HasIndex(p => p.ShipmentId);

        builder.HasOne(p => p.Profile)
            .WithMany()
            .HasForeignKey(p => p.LogisticsPartnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Shipment)
            .WithMany()
            .HasForeignKey(p => p.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.GeneratedBy)
            .WithMany()
            .HasForeignKey(p => p.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DemandForecastConfiguration : IEntityTypeConfiguration<DemandForecast>
{
    public void Configure(EntityTypeBuilder<DemandForecast> builder)
    {
        builder.ToTable("LogisticsDemandForecasts");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Scope).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.ScopeLabel).IsRequired().HasMaxLength(160);
        builder.Property(f => f.Metric).IsRequired().HasMaxLength(30);
        builder.Property(f => f.Granularity).IsRequired().HasMaxLength(10);
        builder.Property(f => f.Method).IsRequired().HasMaxLength(60);
        builder.Property(f => f.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(f => f.Confidence).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.AssumptionsJson).HasColumnType("text");
        builder.Property(f => f.CreatedAt).IsRequired();

        builder.HasIndex(f => new { f.LogisticsPartnerProfileId, f.CreatedAt });
        builder.HasIndex(f => new { f.Scope, f.ScopeId });

        builder.HasOne(f => f.Profile)
            .WithMany()
            .HasForeignKey(f => f.LogisticsPartnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.GeneratedBy)
            .WithMany()
            .HasForeignKey(f => f.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Points)
            .WithOne(p => p.DemandForecast)
            .HasForeignKey(p => p.DemandForecastId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DemandForecastPointConfiguration : IEntityTypeConfiguration<DemandForecastPoint>
{
    public void Configure(EntityTypeBuilder<DemandForecastPoint> builder)
    {
        builder.ToTable("LogisticsDemandForecastPoints");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PeriodDate).IsRequired();
        builder.HasIndex(p => new { p.DemandForecastId, p.PeriodDate });
    }
}

public class RouteOptimizationRunConfiguration : IEntityTypeConfiguration<RouteOptimizationRun>
{
    public void Configure(EntityTypeBuilder<RouteOptimizationRun> builder)
    {
        builder.ToTable("RouteOptimizationRuns");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Method).IsRequired().HasMaxLength(60);
        builder.Property(r => r.Objective).IsRequired().HasMaxLength(30);
        builder.Property(r => r.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.Confidence).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.OriginalDistanceKm).HasColumnType("numeric(10,2)");
        builder.Property(r => r.ProposedDistanceKm).HasColumnType("numeric(10,2)");
        builder.Property(r => r.DistanceSavingKm).HasColumnType("numeric(10,2)");
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => new { r.LogisticsPartnerProfileId, r.CreatedAt });
        builder.HasIndex(r => new { r.DeliveryRouteId, r.Status });

        builder.HasOne(r => r.Profile)
            .WithMany()
            .HasForeignKey(r => r.LogisticsPartnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.DeliveryRoute)
            .WithMany()
            .HasForeignKey(r => r.DeliveryRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.GeneratedBy)
            .WithMany()
            .HasForeignKey(r => r.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AppliedBy)
            .WithMany()
            .HasForeignKey(r => r.AppliedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Stops)
            .WithOne(s => s.RouteOptimizationRun)
            .HasForeignKey(s => s.RouteOptimizationRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RouteOptimizationRunStopConfiguration : IEntityTypeConfiguration<RouteOptimizationRunStop>
{
    public void Configure(EntityTypeBuilder<RouteOptimizationRunStop> builder)
    {
        builder.ToTable("RouteOptimizationRunStops");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Label).IsRequired().HasMaxLength(400);
        builder.Property(s => s.DistanceFromPreviousKm).HasColumnType("numeric(10,2)");

        builder.HasIndex(s => new { s.RouteOptimizationRunId, s.ProposedSequence });
    }
}

public class WarehouseAllocationRecommendationConfiguration : IEntityTypeConfiguration<WarehouseAllocationRecommendation>
{
    public void Configure(EntityTypeBuilder<WarehouseAllocationRecommendation> builder)
    {
        builder.ToTable("WarehouseAllocationRecommendations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Objective).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Sku).HasMaxLength(80);
        builder.Property(r => r.Method).IsRequired().HasMaxLength(60);
        builder.Property(r => r.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.Confidence).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.RecommendedWarehouseCode).HasMaxLength(40);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => new { r.LogisticsPartnerProfileId, r.CreatedAt });

        builder.HasOne(r => r.Profile)
            .WithMany()
            .HasForeignKey(r => r.LogisticsPartnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.GeneratedBy)
            .WithMany()
            .HasForeignKey(r => r.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.DestinationDistrict)
            .WithMany()
            .HasForeignKey(r => r.DestinationDistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Shipment)
            .WithMany()
            .HasForeignKey(r => r.ShipmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.RecommendedWarehouse)
            .WithMany()
            .HasForeignKey(r => r.RecommendedWarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Options)
            .WithOne(o => o.Recommendation)
            .HasForeignKey(o => o.WarehouseAllocationRecommendationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WarehouseAllocationOptionConfiguration : IEntityTypeConfiguration<WarehouseAllocationOption>
{
    public void Configure(EntityTypeBuilder<WarehouseAllocationOption> builder)
    {
        builder.ToTable("WarehouseAllocationOptions");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.WarehouseCode).IsRequired().HasMaxLength(40);
        builder.Property(o => o.WarehouseName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Rationale).IsRequired().HasMaxLength(1000);

        builder.HasIndex(o => new { o.WarehouseAllocationRecommendationId, o.Rank });
    }
}
