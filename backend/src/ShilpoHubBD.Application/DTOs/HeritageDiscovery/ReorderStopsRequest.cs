namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class ReorderStopsRequest
{
    // Ordered list of stop IDs, in the new desired order.
    public List<Guid> StopIds { get; set; } = new();
}
