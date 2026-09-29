namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProducerMonthlyReportDto
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string ProducerEmail { get; set; } = string.Empty;

    public int Year { get; set; }
    public int Month { get; set; }

    public int TotalOrders { get; set; }
    public decimal TotalSales { get; set; }
    public decimal NetIncome { get; set; }
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int CancelledOrders { get; set; }
    public decimal CancellationRate { get; set; }
    public int ProductCount { get; set; }
    public int UnitsSold { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public decimal PreviousMonthSales { get; set; }
    public decimal? SalesGrowthPercentage { get; set; }

    public int OverallSalesRank { get; set; }
    public decimal? OverallSalesPercentile { get; set; }

    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? CategoryPosition { get; set; }
    public decimal? CategoryAverageSales { get; set; }

    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public int? DistrictPosition { get; set; }
    public decimal? DistrictAverageSales { get; set; }

    public decimal? PeerAverageGrowthPercentage { get; set; }

    /// <summary>Rule-based flags computed from this report's own numbers (no AI, no new calculation) — see ProducerMonthlyReportService.DetectProblems for the exact thresholds. Empty when nothing stands out.</summary>
    public List<string> DetectedProblems { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}
