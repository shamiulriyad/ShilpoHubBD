namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchAIAnalysisQueryParameters
{
    public string? Type { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
