namespace ShilpoHubBD.Application.DTOs.Auction;

public class AuctionDto
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MinBidIncrement { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public long TimeRemainingSeconds { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public int BidCount { get; set; }
    public List<AuctionBidDto> Bids { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
