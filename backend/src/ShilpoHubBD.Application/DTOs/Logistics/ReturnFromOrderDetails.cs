namespace ShilpoHubBD.Application.DTOs.Logistics;

// What a producer supplies when accepting a customer return: the partner then collects the goods from
// the customer and brings them back to the producer.
public class ReturnFromOrderDetails
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string PickupAddressLine { get; set; } = string.Empty;
    public string PickupCity { get; set; } = string.Empty;
    public Guid? PickupDistrictId { get; set; }
    public string? ReasonDetail { get; set; }
    public decimal RefundAmount { get; set; }
    public List<ReturnItemInput> Items { get; set; } = new();
}
