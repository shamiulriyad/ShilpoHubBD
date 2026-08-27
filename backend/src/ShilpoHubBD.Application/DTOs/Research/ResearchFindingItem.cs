using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.DTOs.Research;

/// <summary>One structured item returned by a provider analysis method.</summary>
public class ResearchFindingItem
{
    public ResearchAIFindingCategory Category { get; set; } = ResearchAIFindingCategory.Insight;
    public string Heading { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string? Metric { get; set; }
    public double? Score { get; set; }
}
