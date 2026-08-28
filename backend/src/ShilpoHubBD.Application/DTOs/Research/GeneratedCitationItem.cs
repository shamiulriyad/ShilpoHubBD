using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.DTOs.Research;

public class GeneratedCitationItem
{
    public ResearchCitationStyle Style { get; set; }
    public string SourceTitle { get; set; } = string.Empty;
    public string? Authors { get; set; }
    public int? Year { get; set; }
    public string? Container { get; set; }
    public string? Doi { get; set; }
    public string? Url { get; set; }
    public string FormattedCitation { get; set; } = string.Empty;
    public Guid? ResearchPublicationId { get; set; }
}
