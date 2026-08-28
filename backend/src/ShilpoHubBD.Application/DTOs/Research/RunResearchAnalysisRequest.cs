using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Research;

/// <summary>Shared request body for the insights / trends / correlations / report endpoints.</summary>
public class RunResearchAnalysisRequest
{
    public string? Title { get; set; }
    public List<string> ResearchQuestions { get; set; } = new();
    public string? Notes { get; set; }
    public Guid? DatasetId { get; set; }
    public Guid? ResearchPaperId { get; set; }
    public List<ResearchDataPointDto> SelectedData { get; set; } = new();
    public DateTime? RangeStart { get; set; }
    public DateTime? RangeEnd { get; set; }
}
