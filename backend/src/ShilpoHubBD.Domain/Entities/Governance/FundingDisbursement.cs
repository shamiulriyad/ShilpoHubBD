using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>A scheduled or completed payment against an approved <see cref="FundingApplication"/>.</summary>
public class FundingDisbursement
{
    public Guid Id { get; set; }

    public Guid FundingApplicationId { get; set; }
    public FundingApplication Application { get; set; } = null!;

    public decimal Amount { get; set; }
    public FundingDisbursementMethod Method { get; set; }
    public FundingDisbursementStatus Status { get; set; } = FundingDisbursementStatus.Scheduled;

    public DateTime ScheduledFor { get; set; }
    public DateTime? PaidAt { get; set; }

    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public Guid RecordedByUserId { get; set; }
    public User RecordedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
