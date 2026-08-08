using ShilpoHubBD.Domain.Entities.CustomOrders;

namespace ShilpoHubBD.Application.DTOs.CustomOrders;

public class RespondToCustomOrderRequest
{
    public CustomOrderStatus Status { get; set; }
    public decimal? QuotedPrice { get; set; }
    public string? ResponseMessage { get; set; }
}
