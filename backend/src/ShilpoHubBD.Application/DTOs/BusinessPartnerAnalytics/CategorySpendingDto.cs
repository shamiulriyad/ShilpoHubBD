namespace ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

public class CategorySpendingDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int OrderCount { get; set; }
}
