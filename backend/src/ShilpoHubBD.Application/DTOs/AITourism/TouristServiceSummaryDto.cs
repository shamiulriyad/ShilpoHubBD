namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TouristServiceSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? DurationMinutes { get; set; }
    public string DistrictName { get; set; } = string.Empty;
}
