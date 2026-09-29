using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.ProducerBusiness;

/// <summary>
/// An immutable historical snapshot of one Producer's marketplace performance for one calendar month.
/// Written once after the month closes and never recalculated in place — a later correction should
/// insert a new row, not mutate an existing one, so reports stay auditable.
/// One row per (ProducerId, Year, Month).
/// </summary>
public class ProducerMonthlyReport
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    /// <summary>1-12.</summary>
    public int Month { get; set; }

    public int Year { get; set; }

    public int TotalOrders { get; set; }

    /// <summary>Gross sales value for the month (sum of order item line totals counted as revenue).</summary>
    public decimal TotalSales { get; set; }

    /// <summary>TotalSales net of refunds/cancellations for the month.</summary>
    public decimal NetIncome { get; set; }

    /// <summary>Null when the producer received no reviews in the month.</summary>
    public decimal? AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public int CancelledOrders { get; set; }

    /// <summary>Percentage (0-100) of the month's orders that were cancelled.</summary>
    public decimal CancellationRate { get; set; }

    /// <summary>Number of active products the producer had listed during the month.</summary>
    public int ProductCount { get; set; }

    public int UnitsSold { get; set; }

    /// <summary>Distinct customers who bought from this producer for the first time this month.</summary>
    public int NewCustomers { get; set; }

    /// <summary>Distinct customers who had also bought from this producer in a prior month.</summary>
    public int ReturningCustomers { get; set; }

    /// <summary>TotalSales from the preceding month's report, denormalized here for fast trend queries.</summary>
    public decimal PreviousMonthSales { get; set; }

    /// <summary>Null when PreviousMonthSales is 0 (no baseline to compare against).</summary>
    public decimal? SalesGrowthPercentage { get; set; }

    // ---- Peer positioning -------------------------------------------------
    // Computed once, across every producer who has a report for this same (Year, Month), then frozen
    // here — the peer cohort only grows over time (new producers keep registering), so recomputing
    // these later from scratch would not reproduce what was true when this report was generated.
    // Ranking method mirrors the existing convention in NationalDashboardService.GetDistrictRankingsAsync:
    // order by the metric descending, break ties by Id for full determinism, rank = position + 1.
    // No weighted/composite "score" is invented here — every position below is a plain ordinal rank
    // on a single, existing metric (TotalSales).

    /// <summary>1-based rank by TotalSales among ALL producers with a report this month (1 = highest).</summary>
    public int OverallSalesRank { get; set; }

    /// <summary>Share of peer producers this one outsold, as a percentage (100 = top spot). Null when there is only one producer to rank.</summary>
    public decimal? OverallSalesPercentile { get; set; }

    /// <summary>The category that generated the most of this producer's delivered revenue this month. Null when the producer had no delivered sales this month (nothing to attribute a category to).</summary>
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>1-based rank by TotalSales among producers sharing the same CategoryId this month.</summary>
    public int? CategoryPosition { get; set; }

    /// <summary>Average TotalSales across producers sharing the same CategoryId this month.</summary>
    public decimal? CategoryAverageSales { get; set; }

    /// <summary>The district that generated the most of this producer's delivered revenue this month. Null when the producer had no delivered sales this month.</summary>
    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

    /// <summary>1-based rank by TotalSales among producers sharing the same DistrictId this month.</summary>
    public int? DistrictPosition { get; set; }

    /// <summary>Average TotalSales across producers sharing the same DistrictId this month.</summary>
    public decimal? DistrictAverageSales { get; set; }

    /// <summary>
    /// Average SalesGrowthPercentage of every OTHER producer this month (this producer's own growth
    /// excluded from the average). Null when no other producer has a growth figure to compare against.
    /// </summary>
    public decimal? PeerAverageGrowthPercentage { get; set; }

    public DateTime CreatedAt { get; set; }
}
