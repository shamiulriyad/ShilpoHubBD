namespace ShilpoHubBD.Application.DTOs.TouristBooking;

public class CreateServiceAvailabilitySlotRequest
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int? Capacity { get; set; }
}
