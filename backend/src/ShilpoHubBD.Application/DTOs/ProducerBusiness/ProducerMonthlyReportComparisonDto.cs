namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProducerMonthlyReportComparisonDto
{
    public ProducerMonthlyReportDto CurrentMonth { get; set; } = null!;

    /// <summary>Null when the producer has no report for the month before CurrentMonth (e.g. their first reported month).</summary>
    public ProducerMonthlyReportDto? PreviousMonth { get; set; }

    // ---- Deltas (CurrentMonth - PreviousMonth); all null until PreviousMonth exists. ----
    public decimal? SalesChange { get; set; }
    public int? OrdersChange { get; set; }
    public decimal? NetIncomeChange { get; set; }
    public int? UnitsSoldChange { get; set; }
    public decimal? AverageRatingChange { get; set; }
    public decimal? CancellationRateChange { get; set; }

    /// <summary>CurrentMonth.OverallSalesRank - PreviousMonth.OverallSalesRank. Negative means the producer moved UP the leaderboard (rank numbers count down from #1).</summary>
    public int? OverallSalesRankChange { get; set; }
}
