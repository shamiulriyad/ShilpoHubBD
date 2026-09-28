namespace ShilpoHubBD.Application.DTOs.ProductIntelligence;

/// <summary>
/// The AI layer only ever receives the already-computed <see cref="ProductIntelligenceDto"/> numbers
/// — never raw database access — so it cannot invent a statistic that wasn't actually calculated.
/// </summary>
public class ProductIntelligenceAiContext
{
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string RangeLabel { get; set; } = string.Empty;
    public bool HasSufficientHistory { get; set; }

    public List<ProductIntelligencePeriodDto> Periods { get; set; } = new();
    public decimal AverageRating { get; set; }
    public int TotalReviewCount { get; set; }
    public int CurrentStock { get; set; }
    public int? LowStockThreshold { get; set; }
    public decimal? RevenueGrowthPercent { get; set; }
    public decimal? UnitsGrowthPercent { get; set; }
}

public class ProductIntelligenceAiInsightsDto
{
    public string DemandTrend { get; set; } = string.Empty;
    public string EstimatedNextPeriodDemand { get; set; } = string.Empty;
    public string SalesTrendInterpretation { get; set; } = string.Empty;
    public string InventoryRecommendation { get; set; } = string.Empty;
    public string PricingObservation { get; set; } = string.Empty;
    public List<string> MarketingOpportunities { get; set; } = new();
    public List<string> RiskIndicators { get; set; } = new();

    public bool IsAiGenerated { get; set; }
    public DateTime GeneratedAt { get; set; }

    /// <summary>Always populated — every field above is analysis/estimate/forecast/recommendation, never a guarantee.</summary>
    public string Disclaimer { get; set; } =
        "AI-generated analysis based on the historical data shown above. This is an estimate and recommendation, not a guaranteed future result.";
}
