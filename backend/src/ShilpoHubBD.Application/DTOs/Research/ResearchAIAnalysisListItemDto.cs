namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchAIAnalysisListItemDto
{
    public Guid Id { get; set; }
    public Guid ResearchProjectId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ResultSummary { get; set; } = string.Empty;
    public double? Confidence { get; set; }
    public int FindingCount { get; set; }
    public int CitationCount { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
