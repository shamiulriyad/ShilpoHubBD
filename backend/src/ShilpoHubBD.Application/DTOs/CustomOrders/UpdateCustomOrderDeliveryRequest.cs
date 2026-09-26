namespace ShilpoHubBD.Application.DTOs.CustomOrders;

public class UpdateCustomOrderDeliveryRequest
{
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string ShippingAddressLine { get; set; } = string.Empty;
    public Guid? ShippingDistrictId { get; set; }
}
