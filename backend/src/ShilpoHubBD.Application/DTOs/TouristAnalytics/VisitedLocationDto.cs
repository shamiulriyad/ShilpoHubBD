namespace ShilpoHubBD.Application.DTOs.TouristAnalytics;

public class VisitedLocationDto
{
    public Guid HeritagePlaceId { get; set; }
    public string HeritagePlaceName { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public int VisitCount { get; set; }
    public DateTime FirstVisitedAt { get; set; }
    public DateTime LastVisitedAt { get; set; }
}
