namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchProjectListItemDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Discipline { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string MyRole { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int OpenTaskCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
