namespace ShilpoHubBD.Application.DTOs.AITourism;

public class RoutePlaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
