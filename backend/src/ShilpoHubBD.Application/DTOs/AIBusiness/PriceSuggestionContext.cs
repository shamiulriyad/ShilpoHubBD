namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class PriceSuggestionContext
{
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal? CurrentPrice { get; set; }
    public decimal? CategoryAveragePrice { get; set; }
    public decimal? CategoryMinPrice { get; set; }
    public decimal? CategoryMaxPrice { get; set; }
    public decimal? ProducerAveragePrice { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? DesiredMarginPercent { get; set; }
}
