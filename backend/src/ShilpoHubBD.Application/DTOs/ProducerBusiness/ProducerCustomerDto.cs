namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProducerCustomerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public int TotalItemsPurchased { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime FirstOrderAt { get; set; }
    public DateTime LastOrderAt { get; set; }
}
