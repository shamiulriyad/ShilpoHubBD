namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

/// <summary>One producer's row in the Admin Monthly Producer Intelligence table.</summary>
public class ProducerIntelligenceRowDto
{
    public Guid ReportId { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string ProducerEmail { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }

    public decimal Sales { get; set; }
    public decimal Income { get; set; }
    public int Orders { get; set; }
    public decimal? Rating { get; set; }

    /// <summary>Month-over-month sales growth percentage (null when there's no prior-month baseline).</summary>
    public decimal? Growth { get; set; }

    /// <summary>1-based rank by sales among all producers this month.</summary>
    public int Position { get; set; }
    public decimal? PositionPercentile { get; set; }

    /// <summary>Rule-based flags computed from this report's own numbers — see ProducerMonthlyReportService for the exact thresholds. Empty when nothing stands out.</summary>
    public List<string> DetectedProblems { get; set; } = new();

    /// <summary>The status of this producer's most recently updated Government/NGO support case, or "No active support case".</summary>
    public string SupportStatus { get; set; } = string.Empty;
}

/// <summary>KPI summary for the Admin Monthly Producer Intelligence dashboard, over whichever producers match the current filters.</summary>
public class ProducerIntelligenceDashboardDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }

    public int TotalProducers { get; set; }
    public decimal MonthlySales { get; set; }
    public decimal AverageProducerIncome { get; set; }

    /// <summary>Average of AverageRating across producers that had at least one review this month. Null when none did.</summary>
    public decimal? AverageRating { get; set; }

    /// <summary>Producers with a positive month-over-month sales growth.</summary>
    public int GrowingProducers { get; set; }

    /// <summary>Producers with a negative month-over-month sales growth.</summary>
    public int DecliningProducers { get; set; }

    /// <summary>Producers with at least one detected problem this month.</summary>
    public int ProducersNeedingSupport { get; set; }
}
