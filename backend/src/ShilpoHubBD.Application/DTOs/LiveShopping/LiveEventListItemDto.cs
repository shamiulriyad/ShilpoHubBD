namespace ShilpoHubBD.Application.DTOs.LiveShopping;

public class LiveEventListItemDto
{
    public Guid Id { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public bool HasLiveAuction { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledStartAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public int ReactionCount { get; set; }
}
