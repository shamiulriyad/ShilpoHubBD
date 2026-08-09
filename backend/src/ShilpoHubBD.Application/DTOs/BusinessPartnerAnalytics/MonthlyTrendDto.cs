namespace ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

public class MonthlyTrendDto
{
    public DateTime PeriodStart { get; set; }
    public int Quantity { get; set; }
    public decimal Value { get; set; }
}
