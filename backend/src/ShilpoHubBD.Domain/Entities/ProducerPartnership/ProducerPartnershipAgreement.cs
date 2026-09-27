using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

/// <summary>
/// A time-limited partnership between one Business Partner and one Producer, normally created from
/// a won <see cref="ProducerPartnershipAuctionLot"/>. <see cref="WinningBidAmount"/> (what the auction
/// was won for) and the revenue-share percentages below are deliberately separate concepts — the bid
/// amount is never assumed to equal a share of revenue; an admin sets the shares explicitly.
/// </summary>
public class ProducerPartnershipAgreement
{
    public Guid Id { get; set; }

    public Guid? AuctionId { get; set; }
    public ProducerPartnershipAuction? Auction { get; set; }

    public Guid? AuctionLotId { get; set; }
    public ProducerPartnershipAuctionLot? AuctionLot { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public ProducerPartnershipAgreementStatus Status { get; set; } = ProducerPartnershipAgreementStatus.Pending;

    /// <summary>What the Business Partner's auction bid for this producer actually won at — a historical fact, not a share of revenue.</summary>
    public decimal? WinningBidAmount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>Convenience input: when set and EndDate is not, EndDate = StartDate + this many months, once the partnership activates.</summary>
    public int? PartnershipDurationMonths { get; set; }

    public decimal? ProducerSharePercentage { get; set; }
    public decimal? BusinessPartnerSharePercentage { get; set; }
    public decimal? PlatformFeePercentage { get; set; }
    public ProducerPartnershipSettlementFrequency? SettlementFrequency { get; set; }

    /// <summary>Only meaningful when SettlementFrequency == Custom.</summary>
    public int? CustomSettlementPeriodDays { get; set; }

    /// <summary>Below this, a generated settlement is still recorded but flagged for admin attention rather than approved automatically.</summary>
    public decimal? MinimumSettlementAmount { get; set; }

    public string? AgreementTerms { get; set; }

    public DateTime? ProducerConfirmedAt { get; set; }
    public DateTime? BusinessPartnerConfirmedAt { get; set; }

    public DateTime? EndedAt { get; set; }
    public string? EndReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProducerPartnershipStatusEvent> StatusHistory { get; set; } = new List<ProducerPartnershipStatusEvent>();
    public ICollection<ProducerPartnershipSettlement> Settlements { get; set; } = new List<ProducerPartnershipSettlement>();
}
