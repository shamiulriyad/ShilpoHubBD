namespace ShilpoHubBD.Application.DTOs.Commerce;

public class OrderTrackingDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public List<OrderStatusEventDto> Events { get; set; } = new();
}
