namespace ShilpoHubBD.Application.DTOs.AITourism;

public class RouteOptimizationContext
{
    public List<RoutePlaceDto> Places { get; set; } = new();
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }
}
