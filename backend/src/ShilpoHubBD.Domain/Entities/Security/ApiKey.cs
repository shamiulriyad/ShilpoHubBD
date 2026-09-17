using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Security;

/// <summary>An issued API key for programmatic/API access, shown to the admin exactly once at creation.
/// Only a SHA-256 hash is stored; <see cref="KeyPrefix"/> is a short, safe-to-display identifier.</summary>
public class ApiKey
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
