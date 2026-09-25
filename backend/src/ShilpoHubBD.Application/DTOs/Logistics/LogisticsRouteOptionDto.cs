namespace ShilpoHubBD.Application.DTOs.Logistics;

// A route a logistics partner has planned, as a producer sees it when choosing how a parcel travels.
public class LogisticsRouteOptionDto
{
    public Guid RouteId { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? From { get; set; }
    public string? To { get; set; }
    public string? OriginDistrictName { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public decimal? VehicleCapacityKg { get; set; }
    public decimal? TotalDistanceKm { get; set; }
    public int TotalStops { get; set; }

    /// <summary>Roads / via points and any other detail the partner wrote for this route.</summary>
    public string? Notes { get; set; }
}
