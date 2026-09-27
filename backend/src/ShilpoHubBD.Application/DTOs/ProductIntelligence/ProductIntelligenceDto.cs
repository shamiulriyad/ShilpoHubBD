namespace ShilpoHubBD.Application.DTOs.ProductIntelligence;

/// <summary>One bucket of the selected range (a day, for Last30Days; a month, otherwise). Sales,
/// order, revenue, review and wishlist trends are reported together so they stay aligned by period.</summary>
public class ProductIntelligencePeriodDto
{
    public DateTime PeriodStart { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;

    /// <summary>Delivered units only — same convention as the rest of the platform's revenue reporting.</summary>
    public int UnitsSold { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public int NewReviews { get; set; }
    public int WishlistAdds { get; set; }
}

public class ProductIntelligenceInventoryMovementDto
{
    public DateTime PeriodStart { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public int NetChange { get; set; }
    public int RestockCount { get; set; }
    public int SaleDeductionCount { get; set; }
}

public class ProductIntelligenceInventoryDto
{
    public int CurrentStock { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool IsLowStock { get; set; }
    public List<ProductIntelligenceInventoryMovementDto> Movements { get; set; } = new();
}

/// <summary>Recent performance / growth-decline: the selected range split in half, second vs first.</summary>
public class ProductIntelligenceSummaryDto
{
    public decimal RecentRevenue { get; set; }
    public decimal PriorRevenue { get; set; }
    public decimal? RevenueGrowthPercent { get; set; }

    public int RecentUnitsSold { get; set; }
    public int PriorUnitsSold { get; set; }
    public decimal? UnitsGrowthPercent { get; set; }
}

public class ProductIntelligenceDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }

    public ProductIntelligenceRange Range { get; set; }
    public DateTime RangeStart { get; set; }
    public DateTime RangeEnd { get; set; }

    public decimal AverageRating { get; set; }
    public decimal BayesianRating { get; set; }
    public int TotalReviewCount { get; set; }

    /// <summary>Cumulative total only — the platform doesn't log a view time-series, just a running counter.</summary>
    public int TotalViews { get; set; }

    /// <summary>Current count only for the same reason; the trend (adds per period) IS available, in Periods[].WishlistAdds.</summary>
    public int CurrentWishlistCount { get; set; }

    /// <summary>Always false today — no search-query log exists in this system. Left in the shape so the frontend can show "not available" rather than silently omitting the row.</summary>
    public bool SearchInterestAvailable { get; set; }
    public string? SearchInterestUnavailableReason { get; set; }

    public List<ProductIntelligencePeriodDto> Periods { get; set; } = new();
    public ProductIntelligenceInventoryDto Inventory { get; set; } = new();
    public ProductIntelligenceSummaryDto Summary { get; set; } = new();

    /// <summary>False when there's essentially nothing to analyze (e.g. a brand-new product with no orders) — the frontend and the AI insights endpoint both use this to avoid overstating thin data.</summary>
    public bool HasSufficientHistory { get; set; }
}
