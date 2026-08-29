using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>An append-only audit entry for a <see cref="ReturnRequest"/>.</summary>
public class ReturnEvent
{
    public Guid Id { get; set; }

    public Guid ReturnRequestId { get; set; }
    public ReturnRequest ReturnRequest { get; set; } = null!;

    public ReturnEventType Type { get; set; }

    public ReturnStatus? FromStatus { get; set; }
    public ReturnStatus? ToStatus { get; set; }

    public string? Note { get; set; }

    public Guid? ActorUserId { get; set; }
    public User? Actor { get; set; }

    public DateTime CreatedAt { get; set; }
}
