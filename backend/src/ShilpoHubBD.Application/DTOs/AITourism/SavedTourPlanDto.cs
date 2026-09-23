namespace ShilpoHubBD.Application.DTOs.AITourism;

public class SavedTourPlanSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string? OriginText { get; set; }
    public int DurationDays { get; set; }
    public int PartySize { get; set; }
    public DateTime? StartDate { get; set; }
    public string TransportMode { get; set; } = string.Empty;
    public decimal? TotalEstimatedCost { get; set; }
    public bool IsAiGenerated { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SavedTourPlanDto : SavedTourPlanSummaryDto
{
    public TourPlanRequest Request { get; set; } = new();
    public TourPlanResult Plan { get; set; } = new();
}
