namespace ShilpoHubBD.Application.DTOs.CustomOrders;

public class CreateCustomOrderRequest
{
    public Guid ProducerId { get; set; }
    public Guid? ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Specifications { get; set; } = string.Empty;
    public decimal? Budget { get; set; }
    public DateTime? Deadline { get; set; }

    // Where the finished piece should be delivered.
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string ShippingAddressLine { get; set; } = string.Empty;
    public Guid? ShippingDistrictId { get; set; }
}
