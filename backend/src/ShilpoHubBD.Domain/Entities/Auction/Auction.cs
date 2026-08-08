using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Auction;

public class Auction
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MinBidIncrement { get; set; }

    public AuctionStatus Status { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public Guid? WinnerId { get; set; }
    public User? Winner { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<AuctionBid> Bids { get; set; } = new List<AuctionBid>();
}
