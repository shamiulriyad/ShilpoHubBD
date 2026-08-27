using System.Collections.Generic;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchCitationContext
{
    public string ProjectTitle { get; set; } = string.Empty;
    public ResearchCitationStyle Style { get; set; }
    public List<ResearchCitationSourceDto> Sources { get; set; } = new();
}
