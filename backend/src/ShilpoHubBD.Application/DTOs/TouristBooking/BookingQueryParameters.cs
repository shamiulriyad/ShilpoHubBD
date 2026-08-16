using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Application.DTOs.TouristBooking;

public class BookingQueryParameters
{
    public BookingStatus? Status { get; set; }
    public BookingType? Type { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
