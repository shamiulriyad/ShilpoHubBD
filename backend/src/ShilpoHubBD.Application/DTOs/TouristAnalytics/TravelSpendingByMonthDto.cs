namespace ShilpoHubBD.Application.DTOs.TouristAnalytics;

public class TravelSpendingByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalSpent { get; set; }
    public int BookingCount { get; set; }
}
