namespace ShilpoHubBD.Application.DTOs.AITourism;

public class HeritageFestivalSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DistrictName { get; set; } = string.Empty;
}
