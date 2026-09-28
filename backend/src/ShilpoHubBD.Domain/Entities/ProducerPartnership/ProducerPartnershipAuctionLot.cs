using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

/// <summary>
/// One producer entered into an auction round. CurrentHighestBid/CurrentHighestBidderId/BidCount
/// are denormalized for fast reads and are only ever changed by an atomic, guarded update
/// (see <c>ProducerPartnershipAuctionLotRepository.PlaceBidAsync</c>) so a slower concurrent bid can
/// never silently overwrite a faster higher one.
/// </summary>
public class ProducerPartnershipAuctionLot
{
    public Guid Id { get; set; }

    public Guid AuctionId { get; set; }
    public ProducerPartnershipAuction Auction { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public decimal StartingBid { get; set; }
    public decimal? CurrentHighestBid { get; set; }
    public Guid? CurrentHighestBidderId { get; set; }
    public User? CurrentHighestBidder { get; set; }
    public int BidCount { get; set; }

    public ProducerPartnershipAuctionLotStatus Status { get; set; } = ProducerPartnershipAuctionLotStatus.Pending;

    public Guid? WinningBidId { get; set; }
    public ProducerPartnershipAuctionBid? WinningBid { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProducerPartnershipAuctionBid> Bids { get; set; } = new List<ProducerPartnershipAuctionBid>();
}
