namespace ShilpoHubBD.Application.DTOs.TouristAnalytics;

public class BookingStatisticsDto
{
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal TotalSpent { get; set; }
}
