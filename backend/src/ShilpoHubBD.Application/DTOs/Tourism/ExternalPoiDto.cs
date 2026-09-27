using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Application.DTOs.Tourism;

// One OpenStreetMap object after normalisation and filtering. Fields OSM does not carry are null --
// nothing here is ever filled in by guesswork.
// Which part of the OpenStreetMap data a fetch/import covers. Each is a separate, small Overpass
// request (the one giant query is what kept timing out on the public servers).
public enum ExternalPoiScope
{
    All,
    Accommodation,
    Places,
}

public class ExternalPoiDto
{
    public string ExternalId { get; set; } = string.Empty;   // "node/123", "way/456"
    public string Name { get; set; } = string.Empty;
    public TourismLocationType Type { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? Area { get; set; }
    public string? Upazila { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? OpeningHours { get; set; }
    public string? Description { get; set; }
    // How many useful tags the object carries -- used to keep the best-documented ones when capping.
    public int TagCount { get; set; }
}

public class TourismSyncResultDto
{
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public bool Attempted { get; set; }
    public bool Succeeded { get; set; }
    public int Fetched { get; set; }
    public int Added { get; set; }
    public int Updated { get; set; }
    public int SkippedDuplicates { get; set; }
    public string? Message { get; set; }
}

public class NearbyAccommodationDto : TourismLocationDto
{
    public double DistanceKm { get; set; }
}

public class NearbyAccommodationsDto
{
    public double? CenterLatitude { get; set; }
    public double? CenterLongitude { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public double RadiusKm { get; set; }
    // True while an OpenStreetMap import for this area is still running; the client asks again shortly.
    public bool IsImporting { get; set; }
    public string? Message { get; set; }
    public List<NearbyAccommodationDto> Items { get; set; } = new();
}
