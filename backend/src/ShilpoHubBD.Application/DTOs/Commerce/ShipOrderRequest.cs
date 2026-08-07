namespace ShilpoHubBD.Application.DTOs.Commerce;

public class ShipOrderRequest
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
}
