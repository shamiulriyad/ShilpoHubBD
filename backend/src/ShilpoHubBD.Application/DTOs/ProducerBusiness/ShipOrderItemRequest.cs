namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ShipOrderItemRequest
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
}
