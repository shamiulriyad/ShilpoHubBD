using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.DTOs.ProducerPartnership;

public class GenerateProducerPartnershipSettlementRequest
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class RejectProducerPartnershipSettlementRequest
{
    public string? Reason { get; set; }
}

public class ProducerPartnershipSettlementQueryParameters
{
    public Guid? AgreementId { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProducerPartnershipSettlementDto
{
    public Guid Id { get; set; }
    public Guid AgreementId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string BusinessPartnerName { get; set; } = string.Empty;

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
    public int OrderCount { get; set; }
    public bool BelowMinimumThreshold { get; set; }

    public ProducerPartnershipSettlementStatus Status { get; set; }
    public string? PayoutReference { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime CalculatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
