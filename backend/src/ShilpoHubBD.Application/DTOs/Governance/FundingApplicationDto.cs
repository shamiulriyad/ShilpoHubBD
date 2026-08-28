namespace ShilpoHubBD.Application.DTOs.Governance;

public class FundingApplicationDto
{
    public Guid Id { get; set; }
    public Guid FundingProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;

    public string ApplicantType { get; set; } = string.Empty;
    public Guid? ApplicantUserId { get; set; }
    public Guid? ApplicantVillageId { get; set; }
    public string ApplicantLabel { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }

    public string Purpose { get; set; } = string.Empty;
    public string? Justification { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    public DateTime SubmittedAt { get; set; }
    public DateTime? DecisionAt { get; set; }
    public string? DecisionByName { get; set; }
    public string? DecisionNotes { get; set; }

    public string RepaymentStatus { get; set; } = string.Empty;
    public decimal OutstandingBalance { get; set; }
    public decimal TotalRepaid { get; set; }
    public DateTime? NextRepaymentDueAt { get; set; }

    public decimal TotalDisbursed { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<FundingApplicationReviewDto> Reviews { get; set; } = new();
    public List<FundingDisbursementDto> Disbursements { get; set; } = new();
    public List<FundingApplicationEventDto> Events { get; set; } = new();
}

public class FundingApplicationReviewDto
{
    public Guid Id { get; set; }
    public Guid ReviewerUserId { get; set; }
    public string? ReviewerName { get; set; }
    public string Decision { get; set; } = string.Empty;
    public int? Score { get; set; }
    public decimal? RecommendedAmount { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FundingDisbursementDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledFor { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public string? RecordedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FundingApplicationEventDto
{
    public string Type { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FundingApplicationListItemDto
{
    public Guid Id { get; set; }
    public Guid FundingProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public string ApplicantType { get; set; } = string.Empty;
    public string ApplicantLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string RepaymentStatus { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
