namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class MaterialRequirementDto
{
    public string MaterialName { get; set; } = string.Empty;
    public decimal TotalQuantityNeeded { get; set; }
    public decimal RecommendedBufferQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}
