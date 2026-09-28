using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.DTOs.ProducerPartnership;

public class AddProducerPartnershipAuctionLotRequest
{
    public Guid ProducerId { get; set; }
}

/// <summary>Lightweight row for a lot listing — no Business Profile composition, to keep a full-auction list cheap.</summary>
public class ProducerPartnershipAuctionLotListItemDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public decimal StartingBid { get; set; }
    public decimal? CurrentHighestBid { get; set; }
    public int BidCount { get; set; }
    public ProducerPartnershipAuctionLotStatus Status { get; set; }
    public decimal MinimumNextBid { get; set; }

    public bool IsCurrentUserHighestBidder { get; set; }

    // Populated for SuperAdmin only, or once the lot is Awarded (so a bidder can see who won).
    public string? CurrentHighestBidderName { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public decimal? WinningAmount { get; set; }
}

/// <summary>Full lot detail: the lightweight fields plus the producer's Part 2 Business Profile — never raw producer/user fields.</summary>
public class ProducerPartnershipAuctionLotDetailDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid ProducerId { get; set; }

    public ProducerBusinessProfileDto ProducerProfile { get; set; } = null!;

    public decimal StartingBid { get; set; }
    public decimal? CurrentHighestBid { get; set; }
    public int BidCount { get; set; }
    public ProducerPartnershipAuctionLotStatus Status { get; set; }
    public decimal MinimumNextBid { get; set; }

    public bool IsCurrentUserHighestBidder { get; set; }
    public decimal? CurrentUserHighestBid { get; set; }

    public string? CurrentHighestBidderName { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public decimal? WinningAmount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
