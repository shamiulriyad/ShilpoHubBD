namespace ShilpoHubBD.Application.DTOs.Governance;

public record SupportOrganizationDto(Guid Id, Guid UserId, string OrganizationName, string OrganizationType, string RegistrationNumber, string RegistrationAuthority, string? RegistrationDocumentUrl, string OfficialEmail, string OfficialPhone, string Address, IReadOnlyList<string> OperatingDistricts, string OrganizationDetails, string RepresentativeName, string RepresentativeDesignation, string RepresentativeNid, string RepresentativePhone, string Status, string? ReviewNotes, DateTime UpdatedAt);
public record UpsertSupportOrganizationRequest(string OrganizationName, string OrganizationType, string RegistrationNumber, string RegistrationAuthority, string? RegistrationDocumentUrl, string OfficialEmail, string OfficialPhone, string Address, IReadOnlyList<string> OperatingDistricts, string OrganizationDetails, string RepresentativeName, string RepresentativeDesignation, string RepresentativeNid, string RepresentativePhone, bool SubmitForReview = false);
public record ReviewSupportOrganizationRequest(bool Approved, string? Notes);
public record SupportUserOptionDto(Guid Id, string Name, string? Detail);

public class ArtisanSupportCaseDto
{
    public Guid Id { get; set; } public string CaseNumber { get; set; } = ""; public Guid ArtisanUserId { get; set; } public string ArtisanName { get; set; } = "";
    public Guid? OrganizationUserId { get; set; } public string? OrganizationName { get; set; } public bool AssignedByAdmin { get; set; }
    public string ProblemTitle { get; set; } = ""; public string ProblemDescription { get; set; } = ""; public string? District { get; set; } public string? Craft { get; set; }
    public string? AssignedOfficerName { get; set; } public string? AssignedOfficerPhone { get; set; } public string InspectionResult { get; set; } = "";
    public string? IdentityFindings { get; set; } public string? WorkshopFindings { get; set; } public string? CraftAuthenticityFindings { get; set; } public string? ProductToolsFindings { get; set; } public string? ProblemFindings { get; set; }
    public string? RootCause { get; set; } public string? SupportPlan { get; set; } public string? SupportKind { get; set; } public string? FundingSourceType { get; set; } public string? FundingSourceName { get; set; }
    public decimal? SupportAmount { get; set; } public string? SupportValueDescription { get; set; } public DateTime? SupportProvidedAt { get; set; }
    public string ArtisanConfirmation { get; set; } = ""; public string? ArtisanConfirmationNotes { get; set; } public bool HasDispute { get; set; } public bool IsFlagged { get; set; } public string? FlagReason { get; set; }
    public DateTime? MonitoringDueAt { get; set; } public DateTime? ReportDueAt { get; set; } public string Status { get; set; } = ""; public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; }
    public IReadOnlyList<SupportEvidenceDto> Evidence { get; set; } = []; public IReadOnlyList<SupportMonitoringDto> Monitoring { get; set; } = []; public SupportFinalReportDto? FinalReport { get; set; }
}
public record SupportEvidenceDto(Guid Id, string Stage, string FileUrl, string FileName, string ContentType, string? Caption, DateTime CreatedAt);
public record SupportMonitoringDto(Guid Id, DateTime FollowUpDate, string ProductionStatus, string? IncomeMarketImprovement, string? EquipmentCondition, bool ProblemSolved, string? NewIssues, string? Notes, DateTime CreatedAt);
public record SupportFinalReportDto(Guid Id, string OriginalProblem, string VerificationFindings, string ActionTaken, string FundingSupportSource, decimal? AmountValueUsed, string ReceiptsEvidenceSummary, string ArtisanConfirmationSummary, string MonitoringResult, string FinalOutcome, string Recommendation, string ReviewStatus, string? AdminReviewNotes, DateTime SubmittedAt, DateTime? ReviewedAt);
public record CreateArtisanSupportCaseRequest(Guid? ArtisanUserId, string ProblemTitle, string ProblemDescription, string? District, string? Craft);
public record AssignArtisanSupportCaseRequest(Guid OrganizationUserId, string? OfficerName, string? OfficerPhone, DateTime? InspectionDueAt, DateTime? ReportDueAt);
public record RecordSupportInspectionRequest(string Result, string IdentityFindings, string WorkshopFindings, string CraftAuthenticityFindings, string ProductToolsFindings, string ProblemFindings, string RootCause);
public record RecordSupportPlanRequest(string SupportKind, string SupportPlan, string FundingSourceType, string FundingSourceName, decimal? SupportAmount, string? SupportValueDescription);
public record RecordSupportProvidedRequest(DateTime ProvidedAt, decimal? SupportAmount, string? SupportValueDescription, DateTime? MonitoringDueAt, DateTime ReportDueAt);
public record ConfirmArtisanSupportRequest(string Confirmation, string? Notes);
public record AddSupportMonitoringRequest(DateTime FollowUpDate, string ProductionStatus, string? IncomeMarketImprovement, string? EquipmentCondition, bool ProblemSolved, string? NewIssues, string? Notes);
public record SubmitSupportReportRequest(string OriginalProblem, string VerificationFindings, string ActionTaken, string FundingSupportSource, decimal? AmountValueUsed, string ReceiptsEvidenceSummary, string ArtisanConfirmationSummary, string MonitoringResult, string FinalOutcome, string Recommendation);
public record ReviewSupportReportRequest(string Action, string? Notes);
public record FlagSupportCaseRequest(string Reason);
public record CreateSupportEvidenceRequest(string Stage, string FileUrl, string FileName, string ContentType, string? Caption);
public record ArtisanSupportDashboardDto(int ActiveCases, int PendingInspections, int VerifiedCases, int SupportInProgress, int Monitoring, int ReportsDue, int ReportsSubmitted, int BeneficiariesSupported, int PendingOrganizationVerification, int VerifiedOrganizations, int ReportsWaitingReview, int OverdueReports, int Disputes, int FlaggedCases);
