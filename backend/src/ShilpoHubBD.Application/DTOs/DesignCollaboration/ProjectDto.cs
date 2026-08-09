using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Application.DTOs.DesignCollaboration;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string DesignRequirements { get; set; } = string.Empty;
    public CollaborationStatus Status { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<DesignFileDto> Files { get; set; } = new();
    public List<DesignCommentDto> Comments { get; set; } = new();
    public List<DesignRevisionDto> Revisions { get; set; } = new();
    public List<CollaborationStatusEventDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
