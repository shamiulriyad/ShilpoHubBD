using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Configurations;

public class GovReportConfiguration : IEntityTypeConfiguration<GovReport>
{
    public void Configure(EntityTypeBuilder<GovReport> builder)
    {
        builder.ToTable("GovReports");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.ReportType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Summary).IsRequired().HasMaxLength(4000);
        builder.Property(r => r.Highlights).HasMaxLength(4000);
        builder.Property(r => r.PayloadJson).IsRequired().HasColumnType("text");
        builder.Property(r => r.PeriodStart).IsRequired();
        builder.Property(r => r.PeriodEnd).IsRequired();
        builder.Property(r => r.GeneratedAt).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.ReportType);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.PeriodEnd);
        builder.HasIndex(r => r.GeneratedByUserId);

        builder.HasOne(r => r.GeneratedBy)
            .WithMany()
            .HasForeignKey(r => r.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Sections)
            .WithOne(s => s.Report)
            .HasForeignKey(s => s.GovReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GovReportSectionConfiguration : IEntityTypeConfiguration<GovReportSection>
{
    public void Configure(EntityTypeBuilder<GovReportSection> builder)
    {
        builder.ToTable("GovReportSections");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key).IsRequired().HasMaxLength(60);
        builder.Property(s => s.Title).IsRequired().HasMaxLength(160);
        builder.Property(s => s.Narrative).HasMaxLength(2000);
        builder.Property(s => s.ContentJson).IsRequired().HasColumnType("text");

        builder.HasIndex(s => new { s.GovReportId, s.DisplayOrder });
    }
}

public class AnalyticsExportConfiguration : IEntityTypeConfiguration<AnalyticsExport>
{
    public void Configure(EntityTypeBuilder<AnalyticsExport> builder)
    {
        builder.ToTable("AnalyticsExports");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Dataset).IsRequired().HasConversion<string>().HasMaxLength(40);
        builder.Property(e => e.Format).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.FiltersJson).HasColumnType("text");
        builder.Property(e => e.FileUrl).HasMaxLength(1000);
        builder.Property(e => e.FailureReason).HasMaxLength(2000);
        builder.Property(e => e.RequestedAt).IsRequired();

        builder.HasIndex(e => e.Dataset);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RequestedByUserId);
        builder.HasIndex(e => e.RequestedAt);

        builder.HasOne(e => e.RequestedBy)
            .WithMany()
            .HasForeignKey(e => e.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Report)
            .WithMany()
            .HasForeignKey(e => e.GovReportId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class GovForecastConfiguration : IEntityTypeConfiguration<GovForecast>
{
    public void Configure(EntityTypeBuilder<GovForecast> builder)
    {
        builder.ToTable("GovForecasts");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Title).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Method).IsRequired().HasMaxLength(80);
        builder.Property(f => f.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(f => f.AssumptionsJson).HasColumnType("text");
        builder.Property(f => f.BaselineAsOf).IsRequired();
        builder.Property(f => f.GeneratedAt).IsRequired();
        builder.Property(f => f.CreatedAt).IsRequired();

        builder.HasIndex(f => f.GeneratedAt);
        builder.HasIndex(f => f.GeneratedByUserId);

        builder.HasOne(f => f.GeneratedBy)
            .WithMany()
            .HasForeignKey(f => f.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Points)
            .WithOne(p => p.Forecast)
            .HasForeignKey(p => p.GovForecastId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GovForecastPointConfiguration : IEntityTypeConfiguration<GovForecastPoint>
{
    public void Configure(EntityTypeBuilder<GovForecastPoint> builder)
    {
        builder.ToTable("GovForecastPoints");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Metric).IsRequired().HasMaxLength(60);
        builder.Property(p => p.Unit).IsRequired().HasMaxLength(20);
        builder.Property(p => p.BaselineValue).HasColumnType("numeric(18,2)");
        builder.Property(p => p.ProjectedValue).HasColumnType("numeric(18,2)");
        builder.Property(p => p.LowerBound).HasColumnType("numeric(18,2)");
        builder.Property(p => p.UpperBound).HasColumnType("numeric(18,2)");
        builder.Property(p => p.Confidence).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(p => new { p.GovForecastId, p.DisplayOrder });
    }
}
