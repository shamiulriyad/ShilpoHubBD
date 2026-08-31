using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class CreateRouteStopRequest
{
    // New stops are always appended to the end of the route; use ReorderStopsAsync to change order.
    public Guid HeritagePlaceId { get; set; }
    public TransportationMode TransportationMode { get; set; } = TransportationMode.Walking;
    public string? Notes { get; set; }
}
