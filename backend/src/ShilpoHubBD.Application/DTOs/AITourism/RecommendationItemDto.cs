namespace ShilpoHubBD.Application.DTOs.AITourism;

public class RecommendationItemDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal Score { get; set; }
}
