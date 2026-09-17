namespace ShilpoHubBD.Application.DTOs.Security;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
