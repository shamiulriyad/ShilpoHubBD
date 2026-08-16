using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.TouristBooking;

public class Booking
{
    public Guid Id { get; set; }
    public int PartySize { get; set; } = 1;
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid ServiceId { get; set; }
    public TouristService Service { get; set; } = null!;

    public Guid AvailabilitySlotId { get; set; }
    public ServiceAvailabilitySlot AvailabilitySlot { get; set; } = null!;

    public Guid TouristId { get; set; }
    public User Tourist { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;
}
