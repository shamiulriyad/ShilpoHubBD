using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// A computed heritage-intelligence index (risk, living-heritage, craft-health, village-survival,
/// youth-participation or climate-risk) for a given scope and period. Scores are produced by a
/// replaceable rule-based provider from live platform signals; the component breakdown makes each
/// score explainable.
/// </summary>
public class HeritageIndexRecord
{
    public Guid Id { get; set; }

    public HeritageIndexType IndexType { get; set; }
    public HeritageIndexScope Scope { get; set; }

    /// <summary>DistrictId or VillageId for those scopes; null for National and Craft.</summary>
    public Guid? ScopeId { get; set; }

    /// <summary>Craft name / district name / village name / "National".</summary>
    public string ScopeLabel { get; set; } = string.Empty;

    /// <summary>0–100.</summary>
    public decimal Score { get; set; }

    public HeritageIndexRating Rating { get; set; }

    /// <summary>Provider identity, e.g. "rule-based-heritage-intel-v1".</summary>
    public string Method { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime ComputedAt { get; set; }

    /// <summary>Raw signal bag the score was derived from, serialised as JSON.</summary>
    public string? SignalsJson { get; set; }

    public string? Notes { get; set; }

    public Guid GeneratedByUserId { get; set; }
    public User GeneratedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<HeritageIndexComponent> Components { get; set; } = new List<HeritageIndexComponent>();
}
