namespace ShilpoHubBD.Application.DTOs.TouristBooking;

public class UpdateServiceAvailabilitySlotRequest
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
}
