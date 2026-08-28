namespace ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

public class SpendingAnalyticsDto
{
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<MonthlyTrendDto> MonthlySpending { get; set; } = new();
    public List<CategorySpendingDto> SpendingByCategory { get; set; } = new();
}
