using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.TouristBooking;

public class TouristService
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BookingType Type { get; set; }
    public decimal Price { get; set; }
    public int? DurationMinutes { get; set; }
    public int DefaultCapacity { get; set; } = 1;
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public Guid DistrictId { get; set; }
    public District District { get; set; } = null!;

    public ICollection<ServiceAvailabilitySlot> AvailabilitySlots { get; set; } = new List<ServiceAvailabilitySlot>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
