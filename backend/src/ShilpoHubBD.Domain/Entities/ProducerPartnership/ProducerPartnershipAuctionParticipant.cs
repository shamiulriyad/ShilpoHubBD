using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

/// <summary>A Business Partner's registration to participate (bid) in one auction round, subject to admin approval.</summary>
public class ProducerPartnershipAuctionParticipant
{
    public Guid Id { get; set; }

    public Guid AuctionId { get; set; }
    public ProducerPartnershipAuction Auction { get; set; } = null!;

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public ProducerPartnershipAuctionParticipantStatus Status { get; set; } = ProducerPartnershipAuctionParticipantStatus.Applied;

    public DateTime AppliedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public Guid? DecidedByUserId { get; set; }
    public User? DecidedBy { get; set; }
    public string? DecisionNotes { get; set; }
}
