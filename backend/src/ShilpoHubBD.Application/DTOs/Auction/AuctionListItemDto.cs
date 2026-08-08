namespace ShilpoHubBD.Application.DTOs.Auction;

public class AuctionListItemDto
{
    public Guid Id { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public long TimeRemainingSeconds { get; set; }
    public int BidCount { get; set; }
}
