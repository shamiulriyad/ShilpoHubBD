namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class NearbyHeritagePlaceQueryParameters
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusKm { get; set; } = 25;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
