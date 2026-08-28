namespace ShilpoHubBD.Application.DTOs.TouristBooking;

public class CreateBookingRequest
{
    public Guid ServiceId { get; set; }
    public Guid AvailabilitySlotId { get; set; }
    public int PartySize { get; set; } = 1;
    public string? Notes { get; set; }
}
