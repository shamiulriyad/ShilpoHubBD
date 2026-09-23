namespace ShilpoHubBD.Application.DTOs.AITourism;

public class RouteResultDto
{
    public double DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
    public List<GeoPointDto> Geometry { get; set; } = new();
}
