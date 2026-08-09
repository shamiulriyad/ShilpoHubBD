using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Application.DTOs.DesignCollaboration;

public class CollaborationStatusEventDto
{
    public CollaborationStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
