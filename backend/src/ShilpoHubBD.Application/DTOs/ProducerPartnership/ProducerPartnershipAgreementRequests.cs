namespace ShilpoHubBD.Application.DTOs.ProducerPartnership;

/// <summary>Admin-only manual creation, for a partnership that didn't come from an auction win.</summary>
public class CreateProducerPartnershipAgreementRequest
{
    public Guid? AuctionId { get; set; }
    public Guid ProducerId { get; set; }
    public Guid BusinessPartnerId { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? PartnershipDurationMonths { get; set; }

    public decimal? WinningBidAmount { get; set; }
    public string? AgreementTerms { get; set; }
}

/// <summary>
/// Admin sets/updates the business terms while the agreement is still Pending. The bid amount and
/// the revenue shares are separate concepts — a high winning bid does not imply a particular split.
/// </summary>
public class UpdateProducerPartnershipAgreementTermsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? PartnershipDurationMonths { get; set; }

    public decimal? ProducerSharePercentage { get; set; }
    public decimal? BusinessPartnerSharePercentage { get; set; }
    public decimal? PlatformFeePercentage { get; set; }

    /// <summary>Monthly, Quarterly, Biannual or Custom (see CustomSettlementPeriodDays).</summary>
    public string? SettlementFrequency { get; set; }

    /// <summary>Only used when SettlementFrequency is Custom — the settlement period length in days.</summary>
    public int? CustomSettlementPeriodDays { get; set; }

    /// <summary>Below this, a generated settlement is still recorded but flagged for admin attention rather than approved automatically.</summary>
    public decimal? MinimumSettlementAmount { get; set; }

    public string? AgreementTerms { get; set; }
}

public class TerminateProducerPartnershipAgreementRequest
{
    public string? Reason { get; set; }
}

public class ProducerPartnershipAgreementQueryParameters
{
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
