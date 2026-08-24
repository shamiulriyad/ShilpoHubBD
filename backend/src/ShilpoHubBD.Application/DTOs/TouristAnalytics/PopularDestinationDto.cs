namespace ShilpoHubBD.Application.DTOs.TouristAnalytics;

public class PopularDestinationDto
{
    public Guid HeritagePlaceId { get; set; }
    public string HeritagePlaceName { get; set; } = string.Empty;
    public int VisitCount { get; set; }
}
