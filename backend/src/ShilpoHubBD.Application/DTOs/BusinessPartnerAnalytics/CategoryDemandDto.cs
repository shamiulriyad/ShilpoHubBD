namespace ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

public class CategoryDemandDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalQuantityOrdered { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
}
