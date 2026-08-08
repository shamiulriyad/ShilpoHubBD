using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Auction;

public class AuctionBid
{
    public Guid Id { get; set; }

    public Guid AuctionId { get; set; }
    public Auction Auction { get; set; } = null!;

    public Guid BidderId { get; set; }
    public User Bidder { get; set; } = null!;

    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
