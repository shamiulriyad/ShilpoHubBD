using ShilpoHubBD.Domain.Entities.Community;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// A request for support under a <see cref="FundingProgram"/>, tracked from submission through review
/// and decision to disbursement (and, for loans, repayment).
/// </summary>
public class FundingApplication
{
    public Guid Id { get; set; }

    public Guid FundingProgramId { get; set; }
    public FundingProgram Program { get; set; } = null!;

    /// <summary>Short human-facing reference, unique.</summary>
    public string ReferenceCode { get; set; } = string.Empty;

    public FundingApplicantType ApplicantType { get; set; }

    /// <summary>Set when the applicant is a platform user (producer / student).</summary>
    public Guid? ApplicantUserId { get; set; }
    public User? ApplicantUser { get; set; }

    /// <summary>Set when the applicant is a village.</summary>
    public Guid? ApplicantVillageId { get; set; }
    public Village? ApplicantVillage { get; set; }

    public string ApplicantLabel { get; set; } = string.Empty;

    public FundingApplicationStatus Status { get; set; } = FundingApplicationStatus.Submitted;

    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }

    public string Purpose { get; set; } = string.Empty;
    public string? Justification { get; set; }

    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    public DateTime SubmittedAt { get; set; }
    public DateTime? DecisionAt { get; set; }
    public Guid? DecisionByUserId { get; set; }
    public User? DecisionBy { get; set; }
    public string? DecisionNotes { get; set; }

    // ---- Repayment (loan programmes) --------------------------------
    public LoanRepaymentStatus RepaymentStatus { get; set; } = LoanRepaymentStatus.NotApplicable;
    public decimal OutstandingBalance { get; set; }
    public decimal TotalRepaid { get; set; }
    public DateTime? NextRepaymentDueAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<FundingApplicationReview> Reviews { get; set; } = new List<FundingApplicationReview>();
    public ICollection<FundingDisbursement> Disbursements { get; set; } = new List<FundingDisbursement>();
    public ICollection<FundingApplicationEvent> Events { get; set; } = new List<FundingApplicationEvent>();
}
