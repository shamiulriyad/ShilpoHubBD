namespace ShilpoHubBD.Application.DTOs.CustomOrders;

// Hand a completed custom order to a logistics partner (same idea as shipping a normal order item).
public class ShipCustomOrderRequest
{
    public Guid LogisticsPartnerProfileId { get; set; }
    public Guid? DeliveryRouteId { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Notes { get; set; }
}
