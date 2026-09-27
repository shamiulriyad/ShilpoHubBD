using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

/// <summary>An immutable record of one bid. Never updated after insert — the running "current highest" lives on the lot.</summary>
public class ProducerPartnershipAuctionBid
{
    public Guid Id { get; set; }

    public Guid LotId { get; set; }
    public ProducerPartnershipAuctionLot Lot { get; set; } = null!;

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public decimal Amount { get; set; }

    /// <summary>UTC.</summary>
    public DateTime PlacedAt { get; set; }
}
