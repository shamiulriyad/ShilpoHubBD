namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class MaterialForecastRequest
{
    public int UnitsToProduce { get; set; }
    public List<MaterialUsageDto> MaterialsPerUnit { get; set; } = new();
}
