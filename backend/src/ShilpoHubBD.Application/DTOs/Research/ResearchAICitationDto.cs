namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchAICitationDto
{
    public Guid Id { get; set; }
    public Guid? ResearchPublicationId { get; set; }
    public string Style { get; set; } = string.Empty;
    public string SourceTitle { get; set; } = string.Empty;
    public string? Authors { get; set; }
    public int? Year { get; set; }
    public string? Container { get; set; }
    public string? Doi { get; set; }
    public string? Url { get; set; }
    public string FormattedCitation { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
