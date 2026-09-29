using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.ProducerBusiness;

namespace ShilpoHubBD.Domain.Entities.Governance;

public enum ImpactMetricType { Sales, Orders, NetIncome, UnitsSold, AverageRating, CancellationRate, CustomerRetention, ProductActivity }

/// <summary>Classifies the SIZE of a measured change only — never a claim that the support caused it.</summary>
public enum ImpactStatus { Improved, Declined, NoSignificantChange, InsufficientData }

/// <summary>
/// A before/after comparison of an artisan's monthly performance around when a Government/NGO support
/// case's support was provided (<see cref="ArtisanSupportCase.SupportProvidedAt"/>). One per case —
/// regenerating replaces the metrics with a fresh comparison (using whatever reports and threshold
/// configuration exist at that time) rather than accumulating a new row every time.
/// This measures correlation with the support's timing only; it is not evidence of causation.
/// </summary>
public class ArtisanSupportImpactAssessment
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }
    public ArtisanSupportCase Case { get; set; } = null!;

    /// <summary>The calendar month immediately before the support was provided.</summary>
    public int BeforeYear { get; set; }
    public int BeforeMonth { get; set; }
    /// <summary>Null when the artisan has no monthly report for BeforeYear/BeforeMonth (insufficient data).</summary>
    public Guid? BeforeReportId { get; set; }
    public ProducerMonthlyReport? BeforeReport { get; set; }

    /// <summary>The calendar month the support was provided in.</summary>
    public int AfterYear { get; set; }
    public int AfterMonth { get; set; }
    /// <summary>Null when the artisan has no monthly report for AfterYear/AfterMonth yet (insufficient data).</summary>
    public Guid? AfterReportId { get; set; }
    public ProducerMonthlyReport? AfterReport { get; set; }

    public Guid GeneratedByUserId { get; set; }
    public User GeneratedBy { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }

    public List<ArtisanSupportImpactMetric> Metrics { get; set; } = new();
}

public class ArtisanSupportImpactMetric
{
    public Guid Id { get; set; }

    public Guid AssessmentId { get; set; }
    public ArtisanSupportImpactAssessment Assessment { get; set; } = null!;

    public ImpactMetricType MetricType { get; set; }

    public decimal? BeforeValue { get; set; }
    public decimal? AfterValue { get; set; }
    public decimal? ChangeAbsolute { get; set; }
    /// <summary>Null when BeforeValue is null or zero — no baseline to express a percentage against.</summary>
    public decimal? ChangePercentage { get; set; }
    public ImpactStatus Status { get; set; }
}
