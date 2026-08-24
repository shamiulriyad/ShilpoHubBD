namespace ShilpoHubBD.Domain.Entities.HeritageDiscovery;

public class RouteStop
{
    public Guid Id { get; set; }

    public Guid RouteId { get; set; }
    public HeritageRoute Route { get; set; } = null!;

    public Guid HeritagePlaceId { get; set; }
    public HeritagePlace HeritagePlace { get; set; } = null!;

    public int Order { get; set; }
    public double? DistanceFromPreviousKm { get; set; }
    public int? EstimatedTravelMinutesFromPrevious { get; set; }
    public TransportationMode TransportationMode { get; set; } = TransportationMode.Walking;
    public string? Notes { get; set; }
}
