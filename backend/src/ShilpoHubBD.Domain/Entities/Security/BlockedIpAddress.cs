using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Security;

/// <summary>An IP address blocked from authenticating, enforced by <c>BlockedIpMiddleware</c>.</summary>
public class BlockedIpAddress
{
    public Guid Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    public Guid BlockedByUserId { get; set; }
    public User BlockedBy { get; set; } = null!;

    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
