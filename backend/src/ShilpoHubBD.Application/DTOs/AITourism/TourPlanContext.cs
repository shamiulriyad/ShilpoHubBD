namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TourPlanContext
{
    public string DistrictName { get; set; } = "Bangladesh";
    public int DurationDays { get; set; }
    public int PartySize { get; set; }
    public DateTime? StartDate { get; set; }
    public List<HeritagePlaceSummaryDto> Places { get; set; } = new();
    public List<HeritageFestivalSummaryDto> Festivals { get; set; } = new();
    public List<CulturalEventSummaryDto> Events { get; set; } = new();
    public List<TouristServiceSummaryDto> Services { get; set; } = new();
}
