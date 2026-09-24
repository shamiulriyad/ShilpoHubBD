namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TourPlanResult
{
    public List<TourDayPlanDto> Days { get; set; } = new();
    public List<string> HighlightedFestivals { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public string? AccommodationRecommendation { get; set; }
    // Things the planner could not verify (fees, opening hours, hotel availability, schedules).
    public List<string> UnverifiedNotes { get; set; } = new();
    public TransportEstimateDto? TransportEstimate { get; set; }
    public BudgetPlanResult? EstimatedBudget { get; set; }
    public bool IsAiGenerated { get; set; }
    // Set when the plan was stored in the user's trip history; null if saving failed.
    public Guid? SavedPlanId { get; set; }
}
