namespace ShilpoHubBD.Application.DTOs.Logistics;

// The order facts a shipment needs when a producer hands an item to a logistics partner.
public class OrderHandoffDetails
{
    public Guid OrderId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string DestinationAddressLine { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public Guid? DestinationDistrictId { get; set; }
    public int ParcelCount { get; set; } = 1;
    public decimal? DeclaredValue { get; set; }
    public string? Description { get; set; }
}
