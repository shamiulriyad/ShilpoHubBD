namespace ShilpoHubBD.Domain.Entities.TouristBooking;

public class ServiceAvailabilitySlot
{
    public Guid Id { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Capacity { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid ServiceId { get; set; }
    public TouristService Service { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
