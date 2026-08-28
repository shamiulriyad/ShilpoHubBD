namespace ShilpoHubBD.Application.DTOs.TouristAnalytics;

public class FavoriteBookingCategoryDto
{
    public string BookingType { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalSpent { get; set; }
}
