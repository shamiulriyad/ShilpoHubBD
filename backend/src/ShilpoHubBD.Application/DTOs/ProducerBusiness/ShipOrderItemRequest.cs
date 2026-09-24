namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ShipOrderItemRequest
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;

    // Optional hand-over: when set, a shipment is created for that logistics partner and the
    // tracking number / carrier are filled in from it (TrackingNumber and Carrier may then be empty).
    public Guid? LogisticsPartnerProfileId { get; set; }
}
