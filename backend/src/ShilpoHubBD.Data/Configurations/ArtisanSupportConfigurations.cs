using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Configurations;

public class SupportOrganizationProfileConfiguration : IEntityTypeConfiguration<SupportOrganizationProfile>
{
    public void Configure(EntityTypeBuilder<SupportOrganizationProfile> b)
    {
        b.ToTable("SupportOrganizationProfiles"); b.HasKey(x => x.Id);
        b.Property(x => x.OrganizationName).IsRequired().HasMaxLength(200);
        b.Property(x => x.OrganizationType).IsRequired().HasMaxLength(80);
        b.Property(x => x.RegistrationNumber).IsRequired().HasMaxLength(120);
        b.Property(x => x.RegistrationAuthority).IsRequired().HasMaxLength(200);
        b.Property(x => x.RegistrationDocumentUrl).HasMaxLength(1000);
        b.Property(x => x.OfficialEmail).IsRequired().HasMaxLength(200);
        b.Property(x => x.OfficialPhone).IsRequired().HasMaxLength(40);
        b.Property(x => x.Address).IsRequired().HasMaxLength(500);
        b.Property(x => x.OperatingDistrictsJson).IsRequired().HasColumnType("jsonb");
        b.Property(x => x.OrganizationDetails).IsRequired().HasMaxLength(4000);
        b.Property(x => x.RepresentativeName).IsRequired().HasMaxLength(160);
        b.Property(x => x.RepresentativeDesignation).IsRequired().HasMaxLength(120);
        b.Property(x => x.RepresentativeNid).IsRequired().HasMaxLength(40);
        b.Property(x => x.RepresentativePhone).IsRequired().HasMaxLength(40);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.ReviewNotes).HasMaxLength(2000);
        b.HasIndex(x => x.UserId).IsUnique(); b.HasIndex(x => x.RegistrationNumber).IsUnique();
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ReviewedBy).WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ArtisanSupportCaseConfiguration : IEntityTypeConfiguration<ArtisanSupportCase>
{
    public void Configure(EntityTypeBuilder<ArtisanSupportCase> b)
    {
        b.ToTable("ArtisanSupportCases"); b.HasKey(x => x.Id);
        b.Property(x => x.CaseNumber).IsRequired().HasMaxLength(30); b.HasIndex(x => x.CaseNumber).IsUnique();
        b.Property(x => x.ProblemTitle).IsRequired().HasMaxLength(240); b.Property(x => x.ProblemDescription).IsRequired().HasMaxLength(6000);
        b.Property(x => x.District).HasMaxLength(120); b.Property(x => x.Craft).HasMaxLength(200);
        b.Property(x => x.AssignedOfficerName).HasMaxLength(160); b.Property(x => x.AssignedOfficerPhone).HasMaxLength(40);
        b.Property(x => x.InspectionResult).HasConversion<string>().HasMaxLength(30); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.SupportKind).HasConversion<string>().HasMaxLength(40); b.Property(x => x.ArtisanConfirmation).HasConversion<string>().HasMaxLength(30);
        foreach (var p in new[] { nameof(ArtisanSupportCase.IdentityFindings), nameof(ArtisanSupportCase.WorkshopFindings), nameof(ArtisanSupportCase.CraftAuthenticityFindings), nameof(ArtisanSupportCase.ProductToolsFindings), nameof(ArtisanSupportCase.ProblemFindings), nameof(ArtisanSupportCase.RootCause), nameof(ArtisanSupportCase.SupportPlan), nameof(ArtisanSupportCase.SupportValueDescription), nameof(ArtisanSupportCase.ArtisanConfirmationNotes), nameof(ArtisanSupportCase.FlagReason) }) b.Property(p).HasMaxLength(6000);
        b.Property(x => x.FundingSourceType).HasMaxLength(80); b.Property(x => x.FundingSourceName).HasMaxLength(240); b.Property(x => x.SupportAmount).HasColumnType("numeric(18,2)");
        b.HasIndex(x => new { x.OrganizationUserId, x.Status }); b.HasIndex(x => new { x.ArtisanUserId, x.Status }); b.HasIndex(x => x.ReportDueAt); b.HasIndex(x => x.HasDispute);
        b.HasOne(x => x.Artisan).WithMany().HasForeignKey(x => x.ArtisanUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AssignedByAdmin).WithMany().HasForeignKey(x => x.AssignedByAdminUserId).OnDelete(DeleteBehavior.SetNull);
        b.HasMany(x => x.Evidence).WithOne(x => x.Case).HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.MonitoringEntries).WithOne(x => x.Case).HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.FinalReport).WithOne(x => x.Case).HasForeignKey<ArtisanSupportReport>(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ArtisanSupportEvidenceConfiguration : IEntityTypeConfiguration<ArtisanSupportEvidence>
{
    public void Configure(EntityTypeBuilder<ArtisanSupportEvidence> b) { b.ToTable("ArtisanSupportEvidence"); b.HasKey(x => x.Id); b.Property(x => x.Stage).HasConversion<string>().HasMaxLength(30); b.Property(x => x.FileUrl).IsRequired().HasMaxLength(1000); b.Property(x => x.FileName).IsRequired().HasMaxLength(260); b.Property(x => x.ContentType).IsRequired().HasMaxLength(120); b.Property(x => x.Caption).HasMaxLength(500); b.HasOne(x => x.UploadedBy).WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict); }
}

public class ArtisanSupportMonitoringConfiguration : IEntityTypeConfiguration<ArtisanSupportMonitoring>
{
    public void Configure(EntityTypeBuilder<ArtisanSupportMonitoring> b) { b.ToTable("ArtisanSupportMonitoring"); b.HasKey(x => x.Id); b.Property(x => x.ProductionStatus).IsRequired().HasMaxLength(1000); b.Property(x => x.IncomeMarketImprovement).HasMaxLength(2000); b.Property(x => x.EquipmentCondition).HasMaxLength(2000); b.Property(x => x.NewIssues).HasMaxLength(2000); b.Property(x => x.Notes).HasMaxLength(3000); b.HasOne(x => x.RecordedBy).WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict); }
}

public class ArtisanSupportReportConfiguration : IEntityTypeConfiguration<ArtisanSupportReport>
{
    public void Configure(EntityTypeBuilder<ArtisanSupportReport> b) { b.ToTable("ArtisanSupportReports"); b.HasKey(x => x.Id); foreach (var p in new[] { nameof(ArtisanSupportReport.OriginalProblem), nameof(ArtisanSupportReport.VerificationFindings), nameof(ArtisanSupportReport.ActionTaken), nameof(ArtisanSupportReport.FundingSupportSource), nameof(ArtisanSupportReport.ReceiptsEvidenceSummary), nameof(ArtisanSupportReport.ArtisanConfirmationSummary), nameof(ArtisanSupportReport.MonitoringResult), nameof(ArtisanSupportReport.FinalOutcome), nameof(ArtisanSupportReport.Recommendation), nameof(ArtisanSupportReport.AdminReviewNotes) }) b.Property(p).HasMaxLength(6000); b.Property(x => x.AmountValueUsed).HasColumnType("numeric(18,2)"); b.Property(x => x.ReviewStatus).HasConversion<string>().HasMaxLength(30); b.HasIndex(x => x.ReviewStatus); b.HasOne(x => x.SubmittedBy).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.ReviewedBy).WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.SetNull); }
}
