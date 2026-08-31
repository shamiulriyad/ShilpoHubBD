namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class RouteStopDto
{
    public Guid Id { get; set; }
    public Guid HeritagePlaceId { get; set; }
    public string HeritagePlaceName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Order { get; set; }
    public double? DistanceFromPreviousKm { get; set; }
    public int? EstimatedTravelMinutesFromPrevious { get; set; }
    public string TransportationMode { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
