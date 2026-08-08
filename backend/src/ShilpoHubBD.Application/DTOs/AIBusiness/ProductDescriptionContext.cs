namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class ProductDescriptionContext
{
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? DistrictName { get; set; }
    public List<string> Keywords { get; set; } = new();
    public string Tone { get; set; } = "Warm";
}
