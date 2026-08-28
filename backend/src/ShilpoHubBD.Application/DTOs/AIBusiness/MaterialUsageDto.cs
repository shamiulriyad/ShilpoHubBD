namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class MaterialUsageDto
{
    public string MaterialName { get; set; } = string.Empty;
    public decimal QuantityPerUnit { get; set; }
    public string Unit { get; set; } = string.Empty;
}
