using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Research;

public class GenerateResearchCitationsRequest
{
    public string? Title { get; set; }
    public string Style { get; set; } = string.Empty;
    public List<ResearchCitationSourceDto> Sources { get; set; } = new();

    /// <summary>Project publication ids to pull citation sources from automatically.</summary>
    public List<Guid> PublicationIds { get; set; } = new();
}
