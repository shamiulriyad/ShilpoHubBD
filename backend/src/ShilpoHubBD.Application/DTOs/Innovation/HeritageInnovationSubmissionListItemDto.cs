namespace ShilpoHubBD.Application.DTOs.Innovation;

public class HeritageInnovationSubmissionListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid SubmitterUserId { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public Guid? InnovationPrototypeId { get; set; }
    public int TeamMemberCount { get; set; }
    public int ReviewCount { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
