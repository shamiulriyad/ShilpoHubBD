using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.DTOs.ProducerPartnership;

public class ProducerPartnershipAgreementDto
{
    public Guid Id { get; set; }

    public Guid? AuctionId { get; set; }
    public string? AuctionName { get; set; }
    public Guid? AuctionLotId { get; set; }

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;

    public ProducerPartnershipAgreementStatus Status { get; set; }

    public decimal? WinningBidAmount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? PartnershipDurationMonths { get; set; }

    public decimal? ProducerSharePercentage { get; set; }
    public decimal? BusinessPartnerSharePercentage { get; set; }
    public decimal? PlatformFeePercentage { get; set; }
    public ProducerPartnershipSettlementFrequency? SettlementFrequency { get; set; }
    public int? CustomSettlementPeriodDays { get; set; }
    public decimal? MinimumSettlementAmount { get; set; }

    public string? AgreementTerms { get; set; }

    public DateTime? ProducerConfirmedAt { get; set; }
    public DateTime? BusinessPartnerConfirmedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? EndReason { get; set; }

    public List<ProducerPartnershipStatusEventDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProducerPartnershipAgreementListItemDto
{
    public Guid Id { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string BusinessPartnerName { get; set; } = string.Empty;
    public ProducerPartnershipAgreementStatus Status { get; set; }
    public decimal? WinningBidAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? ProducerSharePercentage { get; set; }
    public decimal? BusinessPartnerSharePercentage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProducerPartnershipStatusEventDto
{
    public ProducerPartnershipAgreementStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
