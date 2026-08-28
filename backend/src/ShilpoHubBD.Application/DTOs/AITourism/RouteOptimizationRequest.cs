namespace ShilpoHubBD.Application.DTOs.AITourism;

public class RouteOptimizationRequest
{
    public List<Guid> PlaceIds { get; set; } = new();
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }
}
