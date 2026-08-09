namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class SupplierRankingRequest
{
    public Guid? CategoryId { get; set; }
    public Guid? DistrictId { get; set; }
    public int MaxResults { get; set; } = 10;
}
