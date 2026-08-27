using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Research;

/// <summary>
/// Fully pre-fetched input for <c>IResearchAIProvider</c>. The provider is a pure function of this
/// context; it performs no data access, so a future Gemini/OpenAI/custom-ML provider is a drop-in.
/// </summary>
public class ResearchAnalysisContext
{
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string? Discipline { get; set; }

    public List<string> ResearchQuestions { get; set; } = new();
    public string? Notes { get; set; }

    public string? DatasetName { get; set; }
    public string? DatasetCategory { get; set; }
    public int? DatasetRecordCount { get; set; }
    public string? DatasetTags { get; set; }

    public string? PaperTitle { get; set; }
    public string? PaperAbstract { get; set; }
    public string? PaperKeywords { get; set; }

    public DateTime? RangeStart { get; set; }
    public DateTime? RangeEnd { get; set; }

    public List<ResearchDataPointDto> SelectedData { get; set; } = new();
}
