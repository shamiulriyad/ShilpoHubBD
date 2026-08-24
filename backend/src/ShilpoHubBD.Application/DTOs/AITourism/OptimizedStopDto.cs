namespace ShilpoHubBD.Application.DTOs.AITourism;

public class OptimizedStopDto
{
    public Guid PlaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public double DistanceFromPreviousKm { get; set; }
}
