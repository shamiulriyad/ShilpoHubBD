namespace ShilpoHubBD.Domain.Entities.Tourism;

// One AI Travel Planner result, kept per user so it can be reopened later. RequestJson / PlanJson
// hold the exact request and generated plan (itinerary, transport route, budget) as returned to
// the user at the time -- a snapshot, so later data changes never rewrite an old trip.
public class SavedTourPlan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string? OriginText { get; set; }
    public int DurationDays { get; set; }
    public int PartySize { get; set; }
    public DateTime? StartDate { get; set; }
    public string TransportMode { get; set; } = string.Empty;
    public decimal? TotalEstimatedCost { get; set; }
    public bool IsAiGenerated { get; set; }
    public string RequestJson { get; set; } = "{}";
    public string PlanJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}
