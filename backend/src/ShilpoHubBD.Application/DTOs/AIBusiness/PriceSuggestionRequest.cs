namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class PriceSuggestionRequest
{
    public Guid? ProductId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? DesiredMarginPercent { get; set; }
}
