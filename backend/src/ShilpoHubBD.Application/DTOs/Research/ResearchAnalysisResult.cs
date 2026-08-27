using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Research;

/// <summary>Common result shape for the insights / trends / correlations / report provider methods.</summary>
public class ResearchAnalysisResult
{
    public string ProviderName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public double? Confidence { get; set; }
    public List<ResearchFindingItem> Items { get; set; } = new();
}
