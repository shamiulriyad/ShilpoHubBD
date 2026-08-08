namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class MaterialForecastResult
{
    public List<MaterialRequirementDto> Requirements { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}
