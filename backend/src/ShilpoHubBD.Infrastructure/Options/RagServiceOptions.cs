namespace ShilpoHubBD.Infrastructure.Options;

public class RagServiceOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8000";
    public string Collection { get; set; } = "shilpohub";
    // A separate, isolated Qdrant collection/Knowledge Base for the Tourist AI Travel Planner --
    // never shares data with the general Heritage Assistant's `Collection` above.
    public string TravelPlannerCollection { get; set; } = "travel-planner";
    public int TimeoutSeconds { get; set; } = 30;
}
