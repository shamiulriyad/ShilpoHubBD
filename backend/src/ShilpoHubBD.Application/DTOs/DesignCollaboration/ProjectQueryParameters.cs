using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Application.DTOs.DesignCollaboration;

public class ProjectQueryParameters
{
    public CollaborationStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
