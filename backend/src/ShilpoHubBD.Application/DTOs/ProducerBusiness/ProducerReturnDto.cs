namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProducerReturnDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime RequestedAt { get; set; }

    public List<ProducerReturnLineDto> Items { get; set; } = new();

    // Value of this producer's items in the order, the order total, and what the customer already paid.
    public decimal ItemsAmount { get; set; }
    public decimal OrderTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal? RefundedAmount { get; set; }

    // Filled once the producer accepts and a logistics return exists.
    public string? ReturnReference { get; set; }
    public string? ReturnStatus { get; set; }
    public string? ReturnCarrier { get; set; }
    public bool CanRespond { get; set; }
}

public class ProducerReturnLineDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
