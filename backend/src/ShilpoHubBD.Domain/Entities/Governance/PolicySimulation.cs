using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// A "what if" policy scenario run by a Government / NGO user (grant / training / tourism campaign /
/// export strategy / employment prediction). Inputs, the baseline captured at run time, and the
/// projected outcomes are all stored so scenarios can be compared later. Projections come from a
/// replaceable rule-based provider — no real ML model yet.
/// </summary>
public class PolicySimulation
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public PolicySimulationType SimulationType { get; set; }
    public HeritageIndexScope Scope { get; set; }
    public Guid? ScopeId { get; set; }
    public string ScopeLabel { get; set; } = string.Empty;

    public PolicySimulationStatus Status { get; set; } = PolicySimulationStatus.Pending;

    public int HorizonMonths { get; set; }

    /// <summary>User-supplied knobs (budget, beneficiaries, duration, intensity, focus craft), as JSON.</summary>
    public string InputsJson { get; set; } = "{}";

    /// <summary>Assumptions the provider applied to produce the numbers, as JSON.</summary>
    public string? AssumptionsJson { get; set; }

    /// <summary>Provider identity, e.g. "rule-based-policy-sim-v1".</summary>
    public string Method { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public PolicySimulationConfidence Confidence { get; set; } = PolicySimulationConfidence.Low;

    // ---- Baseline captured at run time -------------------------------
    public int BaselineProducers { get; set; }
    public int BaselineActiveProducers { get; set; }
    public int BaselineEmployment { get; set; }
    public decimal BaselineExportValue { get; set; }
    public decimal BaselineTourismRevenue { get; set; }
    public decimal BaselineEconomyValue { get; set; }

    public string? Notes { get; set; }
    public string? FailureReason { get; set; }

    public Guid GeneratedByUserId { get; set; }
    public User GeneratedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<PolicySimulationProjection> Projections { get; set; } = new List<PolicySimulationProjection>();
    public ICollection<PolicySimulationRecommendation> Recommendations { get; set; } = new List<PolicySimulationRecommendation>();
}
