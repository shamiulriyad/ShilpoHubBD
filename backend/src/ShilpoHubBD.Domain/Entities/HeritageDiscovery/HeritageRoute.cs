namespace ShilpoHubBD.Domain.Entities.HeritageDiscovery;

public class HeritageRoute
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EstimatedDurationMinutes { get; set; }
    public double TotalDistanceKm { get; set; }
    public bool IsRecommended { get; set; }
    public RouteStatus Status { get; set; } = RouteStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<RouteStop> Stops { get; set; } = new List<RouteStop>();
}
