using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Application.DTOs.TouristBooking;

public class TouristServiceQueryParameters
{
    public string? Search { get; set; }
    public BookingType? Type { get; set; }
    public Guid? DistrictId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
