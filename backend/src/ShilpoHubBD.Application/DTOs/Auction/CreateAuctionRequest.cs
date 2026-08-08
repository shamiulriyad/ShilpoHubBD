namespace ShilpoHubBD.Application.DTOs.Auction;

public class CreateAuctionRequest
{
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal MinBidIncrement { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
