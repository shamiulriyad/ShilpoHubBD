namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

// The logistics partner is the one the customer chose at checkout (Order.LogisticsPartnerProfileId) --
// producers only supply consignment details for the hand-over, they don't pick a partner.
public class ShipOrderItemRequest
{
    public Guid? DeliveryRouteId { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Notes { get; set; }
}
