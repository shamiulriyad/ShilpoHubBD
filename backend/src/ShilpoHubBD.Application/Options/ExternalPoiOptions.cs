namespace ShilpoHubBD.Application.Options;

// Bound to "Tourism:ExternalPoi". Every knob of the OpenStreetMap import lives here.
public class ExternalPoiOptions
{
    public bool Enabled { get; set; } = true;
    public string OverpassUrl { get; set; } = "https://overpass-api.de/api/interpreter";
    // Public mirrors tried in order when the primary is busy or down.
    public List<string> FallbackOverpassUrls { get; set; } = ["https://overpass.kumi.systems/api/interpreter"];
    // How long a page request waits for a fresh import before answering with stored data; the
    // import keeps running in the background and the next view sees it.
    public int MaxWaitSeconds { get; set; } = 8;
    public string UserAgent { get; set; } = "ShilpoHubBD-Tourism/1.0";
    public double DefaultRadiusKm { get; set; } = 15;
    public double MaxRadiusKm { get; set; } = 40;
    // Imported rows younger than this are served from the database without calling Overpass.
    public int RefreshDays { get; set; } = 7;
    // After an attempt that returned nothing or failed, wait this long before asking again.
    public int RetryAfterMinutes { get; set; } = 60;
    public int TimeoutSeconds { get; set; } = 25;
    public int RetryCount { get; set; } = 1;
    // Cap per category so a dense city does not import thousands of restaurants.
    public int MaxPerCategory { get; set; } = 60;
}
