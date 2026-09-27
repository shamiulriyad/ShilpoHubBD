using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

/// <summary>
/// One settlement period's revenue calculation for an Active (at calculation time) partnership.
/// This is calculation + record-keeping only — the platform has no payout integration, so nothing
/// here ever claims money actually moved; <see cref="PayoutReference"/> stays null until one exists.
/// Formula: EligibleRevenue = GrossRevenue - RefundDeductions;
/// PlatformFeeAmount = EligibleRevenue * PlatformFeePercentage / 100;
/// NetPartnershipRevenue = EligibleRevenue - PlatformFeeAmount;
/// ProducerShareAmount / BusinessPartnerShareAmount = NetPartnershipRevenue * their respective %.
/// </summary>
public class ProducerPartnershipSettlement
{
    public Guid Id { get; set; }

    public Guid AgreementId { get; set; }
    public ProducerPartnershipAgreement Agreement { get; set; } = null!;

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public decimal GrossRevenue { get; set; }
    public decimal RefundDeductions { get; set; }
    public decimal PlatformFeePercentageApplied { get; set; }
    public decimal PlatformFeeAmount { get; set; }
    public decimal NetPartnershipRevenue { get; set; }

    public decimal ProducerSharePercentageApplied { get; set; }
    public decimal BusinessPartnerSharePercentageApplied { get; set; }
    public decimal ProducerShareAmount { get; set; }
    public decimal BusinessPartnerShareAmount { get; set; }

    /// <summary>Snapshot of the orders counted as revenue, for auditability against the database.</summary>
    public int OrderCount { get; set; }

    /// <summary>True when BusinessPartnerShareAmount is below the agreement's MinimumSettlementAmount at calculation time — informational only, does not block approval.</summary>
    public bool BelowMinimumThreshold { get; set; }

    public ProducerPartnershipSettlementStatus Status { get; set; } = ProducerPartnershipSettlementStatus.Draft;

    /// <summary>Never populated by this system today — there is no payout integration. Reserved for when one exists.</summary>
    public string? PayoutReference { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime CalculatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
