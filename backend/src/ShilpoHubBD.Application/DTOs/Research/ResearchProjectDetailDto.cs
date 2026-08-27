namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchProjectDetailDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Discipline { get; set; }
    public string? Institution { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string MyRole { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ResearchProjectMemberDto> Members { get; set; } = new();
    public int TaskCount { get; set; }
    public int OpenTaskCount { get; set; }
    public int MilestoneCount { get; set; }
    public int NoteCount { get; set; }
    public int PaperCount { get; set; }
    public int PublicationCount { get; set; }
}
