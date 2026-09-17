namespace ShilpoHubBD.Domain.Entities.Security;

/// <summary>An immutable record of a sensitive action taken through the admin dashboard. Written by
/// services via <c>IAuditLogService.LogAsync</c>, never created or edited through the API directly.</summary>
public class AuditLog
{
    public Guid Id { get; set; }

    public Guid? ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;

    /// <summary>Dotted action code, e.g. "Product.Approved", "AdminUser.Deactivated".</summary>
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }
}
