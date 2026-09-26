namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TourPlanContext
{
    public string DistrictName { get; set; } = "Bangladesh";
    public int DurationDays { get; set; }
    public int PartySize { get; set; }
    public DateTime? StartDate { get; set; }
    public string OriginText { get; set; } = string.Empty;
    public string TransportMode { get; set; } = "Bus";
    public decimal? Budget { get; set; }
    public List<string> Preferences { get; set; } = new();
    public List<HeritagePlaceSummaryDto> Places { get; set; } = new();
    public List<HeritageFestivalSummaryDto> Festivals { get; set; } = new();
    public List<CulturalEventSummaryDto> Events { get; set; } = new();
    public List<TouristServiceSummaryDto> Services { get; set; } = new();
    public List<TourismLocationSummaryDto> TourismLocations { get; set; } = new();
    public List<RagTravelNoteDto> RagNotes { get; set; } = new();
    public DistrictDatasetResult Dataset { get; set; } = new();
    public TransportEstimateDto? TransportEstimate { get; set; }
}
