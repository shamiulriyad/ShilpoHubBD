using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Application.DTOs.TouristBooking;

public class UpdateTouristServiceRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BookingType Type { get; set; }
    public decimal Price { get; set; }
    public int? DurationMinutes { get; set; }
    public int DefaultCapacity { get; set; } = 1;
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public Guid DistrictId { get; set; }
    public bool IsActive { get; set; } = true;
}
