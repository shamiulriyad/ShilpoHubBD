using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.DTOs.Reviews;

/// <summary>What the AI provider is given — text only, no ids, no personal data.</summary>
public class ReviewModerationContext
{
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string ProductName { get; set; } = string.Empty;
}

public class ReviewModerationResultDto
{
    public bool IsNegative { get; set; }
    public bool IsProductRelated { get; set; }
    public ReviewComplaintType ComplaintType { get; set; }
    public ReviewSeverity Severity { get; set; }
    public string IssueSummary { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public bool IsAiGenerated { get; set; }
}
