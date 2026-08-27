using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchAIAnalysisDto
{
    public Guid Id { get; set; }
    public Guid ResearchProjectId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ResearchQuestions { get; set; } = string.Empty;
    public string InputSummary { get; set; } = string.Empty;
    public string ResultSummary { get; set; } = string.Empty;
    public string? ResultJson { get; set; }
    public string? ContextJson { get; set; }
    public double? Confidence { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? DatasetId { get; set; }
    public string? DatasetName { get; set; }
    public Guid? ResearchPaperId { get; set; }
    public string? PaperTitle { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<ResearchAIFindingDto> Findings { get; set; } = new();
    public List<ResearchAICitationDto> Citations { get; set; } = new();
}
