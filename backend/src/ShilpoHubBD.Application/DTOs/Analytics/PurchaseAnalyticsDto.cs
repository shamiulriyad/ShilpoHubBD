namespace ShilpoHubBD.Application.DTOs.Analytics;

public class PurchaseAnalyticsDto
{
    public int TotalOrders { get; set; }
    public int TotalItemsPurchased { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime? FirstPurchaseAt { get; set; }
    public DateTime? LastPurchaseAt { get; set; }
}
