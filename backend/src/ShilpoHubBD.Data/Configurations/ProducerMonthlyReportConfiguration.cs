using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProducerBusiness;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerMonthlyReportConfiguration : IEntityTypeConfiguration<ProducerMonthlyReport>
{
    public void Configure(EntityTypeBuilder<ProducerMonthlyReport> builder)
    {
        builder.ToTable("ProducerMonthlyReports");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TotalSales).HasColumnType("numeric(18,2)");
        builder.Property(r => r.NetIncome).HasColumnType("numeric(18,2)");
        builder.Property(r => r.AverageRating).HasColumnType("numeric(3,2)");
        builder.Property(r => r.CancellationRate).HasColumnType("numeric(5,2)");
        builder.Property(r => r.PreviousMonthSales).HasColumnType("numeric(18,2)");
        builder.Property(r => r.SalesGrowthPercentage).HasColumnType("numeric(9,2)");
        builder.Property(r => r.OverallSalesPercentile).HasColumnType("numeric(5,2)");
        builder.Property(r => r.CategoryAverageSales).HasColumnType("numeric(18,2)");
        builder.Property(r => r.DistrictAverageSales).HasColumnType("numeric(18,2)");
        builder.Property(r => r.PeerAverageGrowthPercentage).HasColumnType("numeric(9,2)");
        builder.Property(r => r.CreatedAt).IsRequired();

        // One snapshot per producer per calendar month.
        builder.HasIndex(r => new { r.ProducerId, r.Year, r.Month }).IsUnique();
        builder.HasIndex(r => new { r.Year, r.Month });
        builder.HasIndex(r => r.CategoryId);
        builder.HasIndex(r => r.DistrictId);

        builder.HasOne(r => r.Producer)
            .WithMany()
            .HasForeignKey(r => r.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.District)
            .WithMany()
            .HasForeignKey(r => r.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProducerMonthlyReportShareConfiguration : IEntityTypeConfiguration<ProducerMonthlyReportShare>
{
    public void Configure(EntityTypeBuilder<ProducerMonthlyReportShare> builder)
    {
        builder.ToTable("ProducerMonthlyReportShares");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SharedAt).IsRequired();

        // Sharing the same report with the same Gov/NGO user twice is a no-op, not a duplicate row.
        builder.HasIndex(s => new { s.ReportId, s.SharedWithUserId }).IsUnique();
        builder.HasIndex(s => s.SharedWithUserId);

        builder.HasOne(s => s.Report)
            .WithMany()
            .HasForeignKey(s => s.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.SharedWithUser)
            .WithMany()
            .HasForeignKey(s => s.SharedWithUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SharedByUser)
            .WithMany()
            .HasForeignKey(s => s.SharedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
