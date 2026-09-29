namespace ShilpoHubBD.Application.DTOs.Governance;

public class ArtisanSupportImpactMetricDto
{
    public string MetricType { get; set; } = string.Empty;
    public decimal? BeforeValue { get; set; }
    public decimal? AfterValue { get; set; }
    public decimal? ChangeAbsolute { get; set; }
    public decimal? ChangePercentage { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ArtisanSupportImpactAssessmentDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }

    public int BeforeYear { get; set; }
    public int BeforeMonth { get; set; }
    public bool BeforeReportAvailable { get; set; }

    public int AfterYear { get; set; }
    public int AfterMonth { get; set; }
    public bool AfterReportAvailable { get; set; }

    public DateTime GeneratedAt { get; set; }
    public List<ArtisanSupportImpactMetricDto> Metrics { get; set; } = new();

    public string Disclaimer { get; set; } =
        "This reflects a measured change in the artisan's monthly performance around the support date. " +
        "It does not prove the support caused the change — other factors may have contributed.";
}

/// <summary>One narrative sentence, tagged with which of the 5 sections it belongs to and whether it's a measured fact or a hedged recommendation.</summary>
public class ProducerImpactFindingDto
{
    public string Category { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

/// <summary>What an IProducerImpactAIProvider returns — not yet persisted.</summary>
public class ProducerImpactNarrativeDto
{
    public bool IsAiGenerated { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public List<ProducerImpactFindingDto> Findings { get; set; } = new();
}

/// <summary>A persisted ProducerImpactAIAnalysis, read back.</summary>
public class ProducerImpactAIAnalysisDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public Guid ImpactAssessmentId { get; set; }
    public bool IsAiGenerated { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public List<ProducerImpactFindingDto> Findings { get; set; } = new();

    public string Disclaimer { get; set; } =
        "AI-generated interpretation of already-verified backend metrics. No calculation is performed here — " +
        "every number was computed beforehand. This does not claim the support caused any observed change.";
}

/// <summary>
/// One row of the Producer Support Impact Report — a completed intervention (support has been
/// provided) with its before/after comparison. Metrics is restricted to the 5 the report asks for
/// (Sales, Orders, NetIncome, AverageRating, CancellationRate); the fuller per-case assessment (which
/// also covers UnitsSold/CustomerRetention/ProductActivity) is available via GET .../impact.
/// </summary>
public class ProducerSupportImpactReportRowDto
{
    public Guid CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public string SupportOrganizationName { get; set; } = string.Empty;
    public string SupportType { get; set; } = string.Empty;
    public DateTime SupportDate { get; set; }

    public int BeforeYear { get; set; }
    public int BeforeMonth { get; set; }
    public int AfterYear { get; set; }
    public int AfterMonth { get; set; }

    /// <summary>0, 1, or 2 — how many of the before/after months actually had a monthly report to compare.</summary>
    public int MonthsUsedForComparison { get; set; }

    public List<ArtisanSupportImpactMetricDto> Metrics { get; set; } = new();

    public string Disclaimer { get; set; } =
        "This reflects a measured change in the artisan's monthly performance around the support date. " +
        "It does not prove the support caused the change — other factors may have contributed.";
}
