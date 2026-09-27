using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.DTOs.ProducerPartnership;

public class DecideProducerPartnershipAuctionParticipantRequest
{
    public bool Approve { get; set; }
    public string? Notes { get; set; }
}

public class ProducerPartnershipAuctionParticipantDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;
    public ProducerPartnershipAuctionParticipantStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }
}
