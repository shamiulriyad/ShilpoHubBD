using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchCitationResult
{
    public string ProviderName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<GeneratedCitationItem> Items { get; set; } = new();
}
