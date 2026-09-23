namespace ShilpoHubBD.Infrastructure.Options;

public class NominatimOptions
{
    public string BaseUrl { get; set; } = "https://nominatim.openstreetmap.org/";
    public string UserAgent { get; set; } = "ShilpoHubBD-TravelPlanner/1.0";
    public int CacheHours { get; set; } = 24;
    public int TimeoutSeconds { get; set; } = 15;
}
