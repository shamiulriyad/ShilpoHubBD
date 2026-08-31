namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class CreateHeritageRouteRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EstimatedDurationMinutes { get; set; }
    public bool IsRecommended { get; set; }
}
