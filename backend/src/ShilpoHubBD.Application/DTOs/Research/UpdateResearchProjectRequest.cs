namespace ShilpoHubBD.Application.DTOs.Research;

public class UpdateResearchProjectRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Discipline { get; set; }
    public string? Institution { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
