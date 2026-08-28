using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>An entry in a <see cref="MonitoringFlag"/>'s audit trail.</summary>
public class MonitoringFlagEvent
{
    public Guid Id { get; set; }

    public Guid MonitoringFlagId { get; set; }
    public MonitoringFlag Flag { get; set; } = null!;

    public MonitoringFlagEventType Type { get; set; }
    public string? Note { get; set; }

    /// <summary>Populated for StatusChanged events.</summary>
    public MonitoringFlagStatus? FromStatus { get; set; }
    public MonitoringFlagStatus? ToStatus { get; set; }

    public Guid ActorUserId { get; set; }
    public User Actor { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
