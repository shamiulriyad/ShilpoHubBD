namespace ShilpoHubBD.Application.DTOs.ProducerPartnership;

public class PlaceProducerPartnershipAuctionBidRequest
{
    public decimal Amount { get; set; }
}

/// <summary>A Business Partner's own bid — never carries another bidder's identity.</summary>
public class ProducerPartnershipAuctionBidDto
{
    public Guid Id { get; set; }
    public Guid LotId { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PlacedAt { get; set; }
    public bool IsCurrentHighest { get; set; }
}

/// <summary>Admin-only bid-history row: includes the bidder's identity.</summary>
public class ProducerPartnershipAuctionBidHistoryEntryDto
{
    public Guid Id { get; set; }
    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PlacedAt { get; set; }
}
