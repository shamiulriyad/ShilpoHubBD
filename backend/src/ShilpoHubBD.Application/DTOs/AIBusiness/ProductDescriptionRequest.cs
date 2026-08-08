namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class ProductDescriptionRequest
{
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public List<string> Keywords { get; set; } = new();
    public string Tone { get; set; } = "Warm";
}
