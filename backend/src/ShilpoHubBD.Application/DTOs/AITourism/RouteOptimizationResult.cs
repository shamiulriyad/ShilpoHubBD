namespace ShilpoHubBD.Application.DTOs.AITourism;

public class RouteOptimizationResult
{
    public List<OptimizedStopDto> Stops { get; set; } = new();
    public double TotalDistanceKm { get; set; }

    /// <summary>Only set when every leg used real road routing -- never a guess built from partial data.</summary>
    public double? TotalEstimatedTravelMinutes { get; set; }

    public double? OriginLatitude { get; set; }
    public double? OriginLongitude { get; set; }
    public string OriginDescription { get; set; } = string.Empty;

    /// <summary>Requested places left out of the route because they have no verified coordinates.</summary>
    public List<string> ExcludedPlaces { get; set; } = new();

    public string Notes { get; set; } = string.Empty;
}
