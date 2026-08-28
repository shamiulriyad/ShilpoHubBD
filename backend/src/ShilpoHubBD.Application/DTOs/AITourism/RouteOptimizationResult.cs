namespace ShilpoHubBD.Application.DTOs.AITourism;

public class RouteOptimizationResult
{
    public List<OptimizedStopDto> Stops { get; set; } = new();
    public double TotalDistanceKm { get; set; }
    public string Notes { get; set; } = string.Empty;
}
