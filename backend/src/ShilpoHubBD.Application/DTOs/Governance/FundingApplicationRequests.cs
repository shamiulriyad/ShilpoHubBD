namespace ShilpoHubBD.Application.DTOs.Governance;

public class CreateFundingApplicationRequest
{
    public Guid FundingProgramId { get; set; }

    /// <summary>Producer, Village, District, Organization or Student.</summary>
    public string ApplicantType { get; set; } = string.Empty;
    public Guid? ApplicantUserId { get; set; }
    public Guid? ApplicantVillageId { get; set; }
    public string ApplicantLabel { get; set; } = string.Empty;

    public decimal RequestedAmount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? Justification { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}

public class SubmitFundingReviewRequest
{
    /// <summary>Approve, Reject or RequestChanges.</summary>
    public string Decision { get; set; } = string.Empty;
    public int? Score { get; set; }
    public decimal? RecommendedAmount { get; set; }
    public string? Comments { get; set; }
}

public class DecideFundingApplicationRequest
{
    /// <summary>Approved or Rejected.</summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>Required when approving; must be within the programme's per-applicant limits and remaining budget.</summary>
    public decimal? ApprovedAmount { get; set; }
    public string? Notes { get; set; }
}

public class ScheduleFundingDisbursementRequest
{
    public decimal Amount { get; set; }

    /// <summary>BankTransfer, MobileMoney, Cheque, InKind or Other.</summary>
    public string Method { get; set; } = "BankTransfer";
    public DateTime ScheduledFor { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class UpdateFundingDisbursementStatusRequest
{
    /// <summary>Scheduled, Paid, Failed or Cancelled.</summary>
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class RecordLoanRepaymentRequest
{
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? NextDueAt { get; set; }
    public string? Notes { get; set; }
}

public class WithdrawFundingApplicationRequest
{
    public string? Reason { get; set; }
}

public class AddFundingApplicationNoteRequest
{
    public string Note { get; set; } = string.Empty;
}

public class FundingApplicationQueryParameters
{
    public Guid? FundingProgramId { get; set; }
    public string? Status { get; set; }
    public string? ApplicantType { get; set; }
    public Guid? ApplicantUserId { get; set; }
    public Guid? ApplicantVillageId { get; set; }
    public string? RepaymentStatus { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
