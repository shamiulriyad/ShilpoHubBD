namespace ShilpoHubBD.Application.DTOs.Research;

public class UpdateResearchPaperRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Abstract { get; set; }
    public string? Authors { get; set; }
    public string? Keywords { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ManuscriptUrl { get; set; }
    public string? TargetVenue { get; set; }
}
