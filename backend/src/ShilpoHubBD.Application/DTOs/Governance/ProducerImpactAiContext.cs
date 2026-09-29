namespace ShilpoHubBD.Application.DTOs.Governance;

/// <summary>
/// Everything the AI provider (or its rule-based fallback) is given — entirely pre-computed values
/// pulled from ArtisanSupportCase/ArtisanSupportImpactAssessment/ProducerMonthlyReport. The provider
/// performs no arithmetic of its own; it only ever sees numbers that already exist here.
/// </summary>
public class ProducerImpactAiContext
{
    public string ProducerName { get; set; } = string.Empty;
    public string SupportOrganizationName { get; set; } = string.Empty;
    public string SupportType { get; set; } = string.Empty;
    public DateTime SupportDate { get; set; }

    public int BeforeYear { get; set; }
    public int BeforeMonth { get; set; }
    public int AfterYear { get; set; }
    public int AfterMonth { get; set; }

    /// <summary>0, 1, or 2 — how many of the before/after months actually had a report. The provider
    /// must treat anything less than 2 as insufficient evidence for a before/after claim.</summary>
    public int MonthsUsedForComparison { get; set; }

    public List<ProducerImpactAiMetricContext> Metrics { get; set; } = new();

    // Peer-positioning context for the After month, if that month's report exists — gives the provider
    // something concrete to base "areas needing attention" / "possible support areas" on beyond the
    // raw before/after deltas. All null when the After report doesn't exist.
    public int? OverallSalesRank { get; set; }
    public decimal? OverallSalesPercentile { get; set; }
    public string? CategoryName { get; set; }
    public int? CategoryPosition { get; set; }
    public decimal? CategoryAverageSales { get; set; }
    public string? DistrictName { get; set; }
    public int? DistrictPosition { get; set; }
    public decimal? DistrictAverageSales { get; set; }
}

public class ProducerImpactAiMetricContext
{
    public string MetricType { get; set; } = string.Empty;
    public decimal? BeforeValue { get; set; }
    public decimal? AfterValue { get; set; }
    public decimal? ChangeAbsolute { get; set; }
    public decimal? ChangePercentage { get; set; }

    /// <summary>Improved / Declined / NoSignificantChange / InsufficientData — already classified by the existing threshold rules; the provider must not reclassify it.</summary>
    public string Status { get; set; } = string.Empty;
}
