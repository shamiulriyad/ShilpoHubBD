namespace ShilpoHubBD.Application.DTOs.TouristBooking;

public class AvailabilitySlotQueryParameters
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool OnlyAvailable { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
