using ShilpoHubBD.Domain.Entities.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Research;

/// <summary>
/// A stored AI Research Assistant request and its result, always linked to a research project.
/// Provider-agnostic: <see cref="ProviderName"/> records which implementation produced the result
/// (currently the rule-based dummy; later Gemini / OpenAI / a custom model).
/// </summary>
public class ResearchAIAnalysis
{
    public Guid Id { get; set; }

    public Guid ResearchProjectId { get; set; }
    public ResearchProject Project { get; set; } = null!;

    public Guid RequestedByUserId { get; set; }
    public User RequestedBy { get; set; } = null!;

    public ResearchAIAnalysisType AnalysisType { get; set; }
    public ResearchAIAnalysisStatus Status { get; set; } = ResearchAIAnalysisStatus.Pending;
    public string ProviderName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>The research questions / prompt supplied by the caller (newline-joined).</summary>
    public string ResearchQuestions { get; set; } = string.Empty;

    /// <summary>Human-readable summary of what was fed into the analysis.</summary>
    public string InputSummary { get; set; } = string.Empty;

    /// <summary>Serialized request context (selected data, dataset ref, paper info).</summary>
    public string? ContextJson { get; set; }

    public string ResultSummary { get; set; } = string.Empty;
    public string? ResultJson { get; set; }
    public double? Confidence { get; set; }
    public string? ErrorMessage { get; set; }

    // Optional cross-links to other modules (kept nullable so deleting the source does not delete history).
    public Guid? DatasetId { get; set; }
    public HeritageDataset? Dataset { get; set; }

    public Guid? ResearchPaperId { get; set; }
    public ResearchPaper? Paper { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<ResearchAIFinding> Findings { get; set; } = new List<ResearchAIFinding>();
    public ICollection<ResearchAICitation> Citations { get; set; } = new List<ResearchAICitation>();
}
