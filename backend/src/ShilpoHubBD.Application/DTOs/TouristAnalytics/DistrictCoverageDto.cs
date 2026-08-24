namespace ShilpoHubBD.Application.DTOs.TouristAnalytics;

public class DistrictCoverageDto
{
    public int VisitedDistrictCount { get; set; }
    public int TotalDistrictCount { get; set; }
    public List<string> VisitedDistrictNames { get; set; } = new();
}
