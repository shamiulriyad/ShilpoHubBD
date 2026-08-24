using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Domain.Entities.Reviews;

public class Review
{
    public Guid Id { get; set; }

    // Exactly one of ProductId/HeritagePlaceId/BookingId is set, identifying what this review is about.
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? HeritagePlaceId { get; set; }
    public HeritagePlace? HeritagePlace { get; set; }

    public Guid? BookingId { get; set; }
    public Booking? Booking { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ReviewImage> Images { get; set; } = new List<ReviewImage>();
}
