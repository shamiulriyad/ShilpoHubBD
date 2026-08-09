using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Application.DTOs.DesignCollaboration;

public class RevisionDecisionRequest
{
    public RevisionStatus Status { get; set; }
    public string? DecisionNotes { get; set; }
}
