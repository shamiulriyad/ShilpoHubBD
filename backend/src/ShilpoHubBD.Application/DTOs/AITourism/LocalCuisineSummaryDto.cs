namespace ShilpoHubBD.Application.DTOs.AITourism;

public class LocalCuisineSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? WhereToTry { get; set; }
    public string DistrictName { get; set; } = string.Empty;
}
