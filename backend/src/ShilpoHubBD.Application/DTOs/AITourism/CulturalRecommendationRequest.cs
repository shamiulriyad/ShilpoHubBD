namespace ShilpoHubBD.Application.DTOs.AITourism;

public class CulturalRecommendationRequest
{
    public Guid? DistrictId { get; set; }
    public List<string> Interests { get; set; } = new();
    public int MaxResults { get; set; } = 10;
}
