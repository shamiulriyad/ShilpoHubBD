namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class PriceSuggestionResult
{
    public decimal SuggestedPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string Rationale { get; set; } = string.Empty;
}
