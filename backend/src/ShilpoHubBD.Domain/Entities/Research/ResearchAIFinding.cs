namespace ShilpoHubBD.Domain.Entities.Research;

/// <summary>One structured item produced by an AI analysis (an insight, trend, correlation, report section, ...).</summary>
public class ResearchAIFinding
{
    public Guid Id { get; set; }

    public Guid ResearchAIAnalysisId { get; set; }
    public ResearchAIAnalysis Analysis { get; set; } = null!;

    public ResearchAIFindingCategory Category { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;

    /// <summary>Optional compact metric, e.g. "r = 0.62", "+18% YoY", "n = 240".</summary>
    public string? Metric { get; set; }

    /// <summary>Optional 0..1 strength / confidence score for this item.</summary>
    public double? Score { get; set; }

    public int DisplayOrder { get; set; }
}
