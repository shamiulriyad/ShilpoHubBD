namespace ShilpoHubBD.Application.DTOs.LiveShopping;

public class CreateLiveEventRequest
{
    public Guid ProductId { get; set; }
    public Guid? AuctionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledStartAt { get; set; }
}
