namespace ShilpoHubBD.Application.DTOs.ProducerPartnership;

public class CreateProducerPartnershipAuctionRequest
{
    public string Name { get; set; } = string.Empty;
    public int AuctionYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? EligibilityCriteria { get; set; }
    public string? BusinessPartnerEligibilityCriteria { get; set; }

    /// <summary>Registration/start date — when Business Partners may begin applying to participate.</summary>
    public DateTime? RegistrationOpensAt { get; set; }
    public DateTime? RegistrationClosesAt { get; set; }
    public DateTime? BiddingOpensAt { get; set; }
    public DateTime? BiddingClosesAt { get; set; }

    /// <summary>Convenience input: when set and <see cref="BiddingClosesAt"/> is not, BiddingClosesAt = BiddingOpensAt + this many hours.</summary>
    public int? AuctionDurationHours { get; set; }

    public decimal? ParticipationFee { get; set; }
    public string Currency { get; set; } = "BDT";

    public decimal MinimumStartingBid { get; set; }
    public decimal MinimumBidIncrement { get; set; }

    /// <summary>Null means unlimited — a Business Partner may win any number of lots.</summary>
    public int? MaxProducersPerBusinessPartner { get; set; }

    public int DefaultPartnershipDurationMonths { get; set; } = 12;
    public decimal? DefaultRevenueSharePercentage { get; set; }
    public string? SettlementRulesDescription { get; set; }
}

public class UpdateProducerPartnershipAuctionRequest
{
    public string? Name { get; set; }

    /// <summary>Draft, Scheduled, RegistrationOpen, Live, Ended, Settled or Cancelled. Prefer the dedicated lifecycle endpoints (schedule/open-registration/go-live/end/cancel) over setting this directly.</summary>
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string? EligibilityCriteria { get; set; }
    public string? BusinessPartnerEligibilityCriteria { get; set; }

    public DateTime? RegistrationOpensAt { get; set; }
    public DateTime? RegistrationClosesAt { get; set; }
    public DateTime? BiddingOpensAt { get; set; }
    public DateTime? BiddingClosesAt { get; set; }

    public decimal? ParticipationFee { get; set; }
    public decimal? MinimumStartingBid { get; set; }
    public decimal? MinimumBidIncrement { get; set; }
    public int? MaxProducersPerBusinessPartner { get; set; }
    public int? DefaultPartnershipDurationMonths { get; set; }
    public decimal? DefaultRevenueSharePercentage { get; set; }
    public string? SettlementRulesDescription { get; set; }
}

public class ProducerPartnershipAuctionQueryParameters
{
    public string? Status { get; set; }
    public int? AuctionYear { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
