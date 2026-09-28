using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

/// <summary>
/// An admin-run annual Producer Partnership Auction round: producers are entered as
/// <see cref="ProducerPartnershipAuctionLot"/>s, eligible Business Partners register as
/// <see cref="ProducerPartnershipAuctionParticipant"/>s, and once <see cref="ProducerPartnershipAuctionStatus.Live"/>
/// they bid for a time-limited <see cref="ProducerPartnershipAgreement"/> with a producer.
/// All timestamps on this entity are UTC (stored as <c>timestamp with time zone</c>) — the same
/// convention used everywhere else in this codebase; clients convert to local time for display.
/// Settlement (revenue-share computation, agreement creation from a won lot) is not implemented yet.
/// </summary>
public class ProducerPartnershipAuction
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int AuctionYear { get; set; }

    public ProducerPartnershipAuctionStatus Status { get; set; } = ProducerPartnershipAuctionStatus.Draft;

    public string Description { get; set; } = string.Empty;

    /// <summary>Free-text rule describing which producers are eligible to be entered as a lot.</summary>
    public string? EligibilityCriteria { get; set; }

    /// <summary>Free-text rule describing which Business Partners are eligible to register/participate.</summary>
    public string? BusinessPartnerEligibilityCriteria { get; set; }

    /// <summary>Registration/start date — when Business Partners may begin applying to participate.</summary>
    public DateTime? RegistrationOpensAt { get; set; }
    public DateTime? RegistrationClosesAt { get; set; }
    public DateTime? BiddingOpensAt { get; set; }
    public DateTime? BiddingClosesAt { get; set; }

    /// <summary>Informational convenience value: BiddingClosesAt − BiddingOpensAt in hours, when both are set.</summary>
    public int? AuctionDurationHours { get; set; }

    public decimal? ParticipationFee { get; set; }
    public string Currency { get; set; } = "BDT";

    /// <summary>Floor for a lot's first bid, unless a lot overrides it with its own StartingBid.</summary>
    public decimal MinimumStartingBid { get; set; }

    /// <summary>Minimum amount a new bid must exceed the current highest bid by.</summary>
    public decimal MinimumBidIncrement { get; set; }

    /// <summary>Null means unlimited — a Business Partner may win any number of lots in this auction.</summary>
    public int? MaxProducersPerBusinessPartner { get; set; }

    /// <summary>Default partnership length awarded to a winning bid, unless overridden per agreement.</summary>
    public int DefaultPartnershipDurationMonths { get; set; } = 12;

    /// <summary>Default revenue-share rate for agreements from this round. Settlement math is not implemented yet.</summary>
    public decimal? DefaultRevenueSharePercentage { get; set; }
    public string? SettlementRulesDescription { get; set; }

    public Guid ManagedByUserId { get; set; }
    public User ManagedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProducerPartnershipAgreement> Agreements { get; set; } = new List<ProducerPartnershipAgreement>();
    public ICollection<ProducerPartnershipAuctionLot> Lots { get; set; } = new List<ProducerPartnershipAuctionLot>();
    public ICollection<ProducerPartnershipAuctionParticipant> Participants { get; set; } = new List<ProducerPartnershipAuctionParticipant>();
}
