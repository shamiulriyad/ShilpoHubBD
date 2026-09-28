using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

public enum OrganizationVerificationStatus { Draft, Pending, Approved, Rejected, Suspended }
public enum ArtisanSupportCaseStatus { Submitted, Assigned, Accepted, InspectionPending, Inspected, SupportPlanned, SupportInProgress, SupportProvided, Monitoring, ReportDue, ReportSubmitted, ClarificationRequested, Closed, Rejected }
public enum ArtisanInspectionResult { NotInspected, Verified, PartiallyVerified, InsufficientEvidence, FraudSuspected }
public enum ArtisanSupportKind { MoneyGrant, ToolsEquipment, RawMaterials, Training, MarketplaceBusinessSupport, WorkshopRepair, HeritagePreservationSupport, Other }
public enum ArtisanSupportConfirmation { Pending, Received, PartiallyReceived, DidNotReceive, ReportProblem }
public enum SupportEvidenceStage { Registration, Request, Inspection, SupportDelivery, Monitoring, FinalReport, Receipt }
public enum SupportReportReviewStatus { NotSubmitted, WaitingReview, ClarificationRequested, Accepted, Flagged }

public class SupportOrganizationProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string OrganizationName { get; set; } = string.Empty;
    public string OrganizationType { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string RegistrationAuthority { get; set; } = string.Empty;
    public string? RegistrationDocumentUrl { get; set; }
    public string OfficialEmail { get; set; } = string.Empty;
    public string OfficialPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string OperatingDistrictsJson { get; set; } = "[]";
    public string OrganizationDetails { get; set; } = string.Empty;
    public string RepresentativeName { get; set; } = string.Empty;
    public string RepresentativeDesignation { get; set; } = string.Empty;
    public string RepresentativeNid { get; set; } = string.Empty;
    public string RepresentativePhone { get; set; } = string.Empty;
    public OrganizationVerificationStatus Status { get; set; } = OrganizationVerificationStatus.Draft;
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ArtisanSupportCase
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid ArtisanUserId { get; set; }
    public User Artisan { get; set; } = null!;
    public Guid? OrganizationUserId { get; set; }
    public User? Organization { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;
    public Guid? AssignedByAdminUserId { get; set; }
    public User? AssignedByAdmin { get; set; }
    public string ProblemTitle { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? Craft { get; set; }
    public string? AssignedOfficerName { get; set; }
    public string? AssignedOfficerPhone { get; set; }
    public ArtisanInspectionResult InspectionResult { get; set; } = ArtisanInspectionResult.NotInspected;
    public string? IdentityFindings { get; set; }
    public string? WorkshopFindings { get; set; }
    public string? CraftAuthenticityFindings { get; set; }
    public string? ProductToolsFindings { get; set; }
    public string? ProblemFindings { get; set; }
    public string? RootCause { get; set; }
    public string? SupportPlan { get; set; }
    public ArtisanSupportKind? SupportKind { get; set; }
    public string? FundingSourceType { get; set; }
    public string? FundingSourceName { get; set; }
    public decimal? SupportAmount { get; set; }
    public string? SupportValueDescription { get; set; }
    public DateTime? SupportProvidedAt { get; set; }
    public ArtisanSupportConfirmation ArtisanConfirmation { get; set; } = ArtisanSupportConfirmation.Pending;
    public string? ArtisanConfirmationNotes { get; set; }
    public bool HasDispute { get; set; }
    public bool IsFlagged { get; set; }
    public string? FlagReason { get; set; }
    public DateTime? MonitoringDueAt { get; set; }
    public DateTime? ReportDueAt { get; set; }
    public ArtisanSupportCaseStatus Status { get; set; } = ArtisanSupportCaseStatus.Submitted;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ArtisanSupportEvidence> Evidence { get; set; } = new();
    public List<ArtisanSupportMonitoring> MonitoringEntries { get; set; } = new();
    public ArtisanSupportReport? FinalReport { get; set; }
}

public class ArtisanSupportEvidence
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public ArtisanSupportCase Case { get; set; } = null!;
    public SupportEvidenceStage Stage { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public Guid UploadedByUserId { get; set; }
    public User UploadedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class ArtisanSupportMonitoring
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public ArtisanSupportCase Case { get; set; } = null!;
    public Guid RecordedByUserId { get; set; }
    public User RecordedBy { get; set; } = null!;
    public string ProductionStatus { get; set; } = string.Empty;
    public string? IncomeMarketImprovement { get; set; }
    public string? EquipmentCondition { get; set; }
    public bool ProblemSolved { get; set; }
    public string? NewIssues { get; set; }
    public string? Notes { get; set; }
    public DateTime FollowedUpAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ArtisanSupportReport
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public ArtisanSupportCase Case { get; set; } = null!;
    public string OriginalProblem { get; set; } = string.Empty;
    public string VerificationFindings { get; set; } = string.Empty;
    public string ActionTaken { get; set; } = string.Empty;
    public string FundingSupportSource { get; set; } = string.Empty;
    public decimal? AmountValueUsed { get; set; }
    public string ReceiptsEvidenceSummary { get; set; } = string.Empty;
    public string ArtisanConfirmationSummary { get; set; } = string.Empty;
    public string MonitoringResult { get; set; } = string.Empty;
    public string FinalOutcome { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public SupportReportReviewStatus ReviewStatus { get; set; } = SupportReportReviewStatus.WaitingReview;
    public string? AdminReviewNotes { get; set; }
    public Guid SubmittedByUserId { get; set; }
    public User SubmittedBy { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
