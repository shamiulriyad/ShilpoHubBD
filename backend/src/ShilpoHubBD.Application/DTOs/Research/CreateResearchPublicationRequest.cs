namespace ShilpoHubBD.Application.DTOs.Research;

public class CreateResearchPublicationRequest
{
    public Guid? ResearchPaperId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public string? Venue { get; set; }
    public string? Type { get; set; }
    public string? Doi { get; set; }
    public string? Url { get; set; }
    public string? Abstract { get; set; }
    public string? Citation { get; set; }
    public DateTime? PublishedOn { get; set; }
    public bool IsPublic { get; set; }
}
