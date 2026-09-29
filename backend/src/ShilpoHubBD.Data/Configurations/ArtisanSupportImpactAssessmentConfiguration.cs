using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Configurations;

public class ArtisanSupportImpactAssessmentConfiguration : IEntityTypeConfiguration<ArtisanSupportImpactAssessment>
{
    public void Configure(EntityTypeBuilder<ArtisanSupportImpactAssessment> b)
    {
        b.ToTable("ArtisanSupportImpactAssessments");
        b.HasKey(x => x.Id);
        b.Property(x => x.GeneratedAt).IsRequired();

        // One current assessment per case — regenerating updates it in place.
        b.HasIndex(x => x.CaseId).IsUnique();

        b.HasOne(x => x.Case).WithMany().HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.BeforeReport).WithMany().HasForeignKey(x => x.BeforeReportId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AfterReport).WithMany().HasForeignKey(x => x.AfterReportId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.GeneratedBy).WithMany().HasForeignKey(x => x.GeneratedByUserId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Metrics).WithOne(m => m.Assessment).HasForeignKey(m => m.AssessmentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ArtisanSupportImpactMetricConfiguration : IEntityTypeConfiguration<ArtisanSupportImpactMetric>
{
    public void Configure(EntityTypeBuilder<ArtisanSupportImpactMetric> b)
    {
        b.ToTable("ArtisanSupportImpactMetrics");
        b.HasKey(x => x.Id);
        b.Property(x => x.MetricType).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.BeforeValue).HasColumnType("numeric(18,2)");
        b.Property(x => x.AfterValue).HasColumnType("numeric(18,2)");
        b.Property(x => x.ChangeAbsolute).HasColumnType("numeric(18,2)");
        b.Property(x => x.ChangePercentage).HasColumnType("numeric(9,2)");

        // One row per metric per assessment.
        b.HasIndex(x => new { x.AssessmentId, x.MetricType }).IsUnique();
    }
}
