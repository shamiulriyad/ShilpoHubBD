namespace ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

public class IndustryInsightDto
{
    public string Industry { get; set; } = string.Empty;
    public int BusinessPartnerCount { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpending { get; set; }
    public decimal AverageOrderValue { get; set; }
}
