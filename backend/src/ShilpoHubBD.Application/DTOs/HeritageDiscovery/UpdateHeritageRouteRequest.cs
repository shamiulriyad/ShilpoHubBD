using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class UpdateHeritageRouteRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EstimatedDurationMinutes { get; set; }
    public bool IsRecommended { get; set; }
    public RouteStatus Status { get; set; }
}
