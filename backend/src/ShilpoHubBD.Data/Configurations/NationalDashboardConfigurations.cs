using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Configurations;

public class NationalDashboardSnapshotConfiguration : IEntityTypeConfiguration<NationalDashboardSnapshot>
{
    public void Configure(EntityTypeBuilder<NationalDashboardSnapshot> builder)
    {
        builder.ToTable("NationalDashboardSnapshots");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Label).IsRequired().HasMaxLength(120);
        builder.Property(s => s.Period).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.PeriodStart).IsRequired();
        builder.Property(s => s.PeriodEnd).IsRequired();
        builder.Property(s => s.CapturedAt).IsRequired();
        builder.Property(s => s.Notes).HasMaxLength(2000);
        builder.Property(s => s.MarketplaceSalesValue).HasColumnType("numeric(18,2)");
        builder.Property(s => s.HeritageEconomyValue).HasColumnType("numeric(18,2)");
        builder.Property(s => s.ExportSalesValue).HasColumnType("numeric(18,2)");
        builder.Property(s => s.TourismRevenue).HasColumnType("numeric(18,2)");
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.Period);
        builder.HasIndex(s => s.PeriodEnd);
        builder.HasIndex(s => s.GeneratedByUserId);

        builder.HasOne(s => s.GeneratedBy)
            .WithMany()
            .HasForeignKey(s => s.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.DistrictStats)
            .WithOne(d => d.Snapshot)
            .HasForeignKey(d => d.NationalDashboardSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DashboardDistrictStatConfiguration : IEntityTypeConfiguration<DashboardDistrictStat>
{
    public void Configure(EntityTypeBuilder<DashboardDistrictStat> builder)
    {
        builder.ToTable("DashboardDistrictStats");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DistrictName).IsRequired().HasMaxLength(120);
        builder.Property(d => d.Division).IsRequired().HasMaxLength(120);
        builder.Property(d => d.SalesValue).HasColumnType("numeric(18,2)");

        builder.HasIndex(d => new { d.NationalDashboardSnapshotId, d.Rank });
        builder.HasIndex(d => d.DistrictId);

        builder.HasOne(d => d.District)
            .WithMany()
            .HasForeignKey(d => d.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
