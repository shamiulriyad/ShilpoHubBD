namespace ShilpoHubBD.Application.DTOs.Auction;

public class AuctionBidDto
{
    public Guid Id { get; set; }
    public Guid BidderId { get; set; }
    public string BidderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
