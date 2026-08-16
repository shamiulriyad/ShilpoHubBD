namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TourPlanResult
{
    public List<TourDayPlanDto> Days { get; set; } = new();
    public List<string> HighlightedFestivals { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}
