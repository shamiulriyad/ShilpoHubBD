namespace ShilpoHubBD.Application.DTOs.Commerce;

public class OrderTrackingDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public List<OrderStatusEventDto> Events { get; set; } = new();
    public string? DeliveryPartnerName { get; set; }
    public string? DeliveryStatus { get; set; }
    public DateTime? ExpectedDeliveryAt { get; set; }
    public List<DeliveryTrackingEventDto> DeliveryEvents { get; set; } = new();
}

public class DeliveryTrackingEventDto
{
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime OccurredAt { get; set; }
}
