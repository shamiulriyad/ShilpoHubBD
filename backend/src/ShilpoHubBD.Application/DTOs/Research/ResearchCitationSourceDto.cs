namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchCitationSourceDto
{
    public string Title { get; set; } = string.Empty;
    public string? Authors { get; set; }
    public int? Year { get; set; }
    public string? Container { get; set; }
    public string? Doi { get; set; }
    public string? Url { get; set; }
    public Guid? ResearchPublicationId { get; set; }
}
