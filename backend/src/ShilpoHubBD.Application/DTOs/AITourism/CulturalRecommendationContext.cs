namespace ShilpoHubBD.Application.DTOs.AITourism;

public class CulturalRecommendationContext
{
    public List<HeritagePlaceSummaryDto> Places { get; set; } = new();
    public List<HeritageFestivalSummaryDto> Festivals { get; set; } = new();
    public List<CulturalEventSummaryDto> Events { get; set; } = new();
    public List<LocalCuisineSummaryDto> Cuisines { get; set; } = new();
    public List<string> Interests { get; set; } = new();
    public int MaxResults { get; set; }
}
