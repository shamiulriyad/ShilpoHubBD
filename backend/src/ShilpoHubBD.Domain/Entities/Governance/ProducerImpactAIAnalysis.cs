using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>Which of the report's five requested sections a finding belongs to.</summary>
public enum ProducerImpactFindingCategory { MainPerformanceChange, IdentifiedProblem, ObservedImprovement, AreaNeedingAttention, PossibleSupportArea }

/// <summary>
/// MeasuredFact = a plain description of what the already-computed numbers show — never invented.
/// Recommendation = a hedged suggestion for what to do next — never phrased as a certainty, and never
/// claiming the support caused a result.
/// </summary>
public enum ProducerImpactFindingKind { MeasuredFact, Recommendation }

/// <summary>
/// A stored narrative interpretation of an ArtisanSupportImpactAssessment's already-computed numbers.
/// The AI (or its rule-based fallback) performs no arithmetic here — every number it saw was already
/// calculated and frozen in the assessment; this only records the prose written about those numbers,
/// kept in its own table so a narrative is never mistaken for verified data. Multiple analyses can
/// exist per case over time (unlike the assessment itself, which is upserted) — each is a distinct,
/// historical interpretation run.
/// </summary>
public class ProducerImpactAIAnalysis
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }
    public ArtisanSupportCase Case { get; set; } = null!;

    public Guid ImpactAssessmentId { get; set; }
    public ArtisanSupportImpactAssessment ImpactAssessment { get; set; } = null!;

    public Guid RequestedByUserId { get; set; }
    public User RequestedBy { get; set; } = null!;

    /// <summary>"Gemini" or "RuleBased" — which implementation produced this run.</summary>
    public string ProviderName { get; set; } = string.Empty;
    public bool IsAiGenerated { get; set; }

    public DateTime GeneratedAt { get; set; }

    public List<ProducerImpactAIFinding> Findings { get; set; } = new();
}

public class ProducerImpactAIFinding
{
    public Guid Id { get; set; }

    public Guid AnalysisId { get; set; }
    public ProducerImpactAIAnalysis Analysis { get; set; } = null!;

    public ProducerImpactFindingCategory Category { get; set; }
    public ProducerImpactFindingKind Kind { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
