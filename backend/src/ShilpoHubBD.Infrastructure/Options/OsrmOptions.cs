namespace ShilpoHubBD.Infrastructure.Options;

public class OsrmOptions
{
    public string BaseUrl { get; set; } = "https://router.project-osrm.org/";
    public string UserAgent { get; set; } = "ShilpoHubBD-TravelPlanner/1.0";
    public int TimeoutSeconds { get; set; } = 15;
    public int CacheHours { get; set; } = 24;
}
