using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.DTOs.ProducerPartnership;

public class ProducerPartnershipAuctionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int AuctionYear { get; set; }
    public ProducerPartnershipAuctionStatus Status { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? EligibilityCriteria { get; set; }
    public string? BusinessPartnerEligibilityCriteria { get; set; }

    public DateTime? RegistrationOpensAt { get; set; }
    public DateTime? RegistrationClosesAt { get; set; }
    public DateTime? BiddingOpensAt { get; set; }
    public DateTime? BiddingClosesAt { get; set; }
    public int? AuctionDurationHours { get; set; }

    public decimal? ParticipationFee { get; set; }
    public string Currency { get; set; } = string.Empty;

    public decimal MinimumStartingBid { get; set; }
    public decimal MinimumBidIncrement { get; set; }
    public int? MaxProducersPerBusinessPartner { get; set; }

    public int DefaultPartnershipDurationMonths { get; set; }
    public decimal? DefaultRevenueSharePercentage { get; set; }
    public string? SettlementRulesDescription { get; set; }

    public Guid ManagedByUserId { get; set; }
    public string ManagedByName { get; set; } = string.Empty;

    public int AgreementCount { get; set; }
    public int LotCount { get; set; }
    public int ParticipantCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProducerPartnershipAuctionListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int AuctionYear { get; set; }
    public ProducerPartnershipAuctionStatus Status { get; set; }
    public DateTime? BiddingOpensAt { get; set; }
    public DateTime? BiddingClosesAt { get; set; }
    public int AgreementCount { get; set; }
    public int LotCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
