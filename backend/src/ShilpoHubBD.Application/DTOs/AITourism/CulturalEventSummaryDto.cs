namespace ShilpoHubBD.Application.DTOs.AITourism;

public class CulturalEventSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string DistrictName { get; set; } = string.Empty;
}
