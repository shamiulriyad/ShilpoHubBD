namespace ShilpoHubBD.Application.DTOs.AITourism;

public class HeritagePlaceSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PlaceType { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsFeatured { get; set; }
    public string DistrictName { get; set; } = string.Empty;
}
