namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class HeritageRouteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EstimatedDurationMinutes { get; set; }
    public double TotalDistanceKm { get; set; }
    public bool IsRecommended { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<RouteStopDto> Stops { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
