namespace ShilpoHubBD.Application.DTOs.Analytics;

public class SpendingByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalSpent { get; set; }
    public int OrderCount { get; set; }
}
