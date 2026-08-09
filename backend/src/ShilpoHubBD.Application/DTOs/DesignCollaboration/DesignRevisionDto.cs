using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Application.DTOs.DesignCollaboration;

public class DesignRevisionDto
{
    public Guid Id { get; set; }
    public int RevisionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public RevisionStatus Status { get; set; }
    public Guid SubmittedByUserId { get; set; }
    public string SubmittedByName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }
    public List<DesignFileDto> Files { get; set; } = new();
}
