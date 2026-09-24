namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TransportEstimateDto
{
    public string Mode { get; set; } = string.Empty;
    public double? EstimatedDistanceKm { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public bool IsVerifiedSchedule { get; set; }
    public string Notes { get; set; } = string.Empty;
    public GeoPointDto? OriginPoint { get; set; }
    public GeoPointDto? DestinationPoint { get; set; }
    public List<GeoPointDto>? RouteGeometry { get; set; }
    // Real operators / services on this route for the chosen mode (sourced; not a live timetable).
    public List<TransportOptionDto> Options { get; set; } = new();
}
