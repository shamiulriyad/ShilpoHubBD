namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchNoteDto
{
    public Guid Id { get; set; }
    public Guid ResearchProjectId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
