namespace ShilpoHubBD.Application.DTOs.TouristBooking;

public class ServiceAvailabilitySlotDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Capacity { get; set; }
    public int BookedCount { get; set; }
    public int RemainingCapacity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
