namespace ShilpoHubBD.Application.DTOs.TouristBooking;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public Guid AvailabilitySlotId { get; set; }
    public DateTime SlotStartAt { get; set; }
    public DateTime SlotEndAt { get; set; }
    public Guid TouristId { get; set; }
    public string TouristName { get; set; } = string.Empty;
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public int PartySize { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
