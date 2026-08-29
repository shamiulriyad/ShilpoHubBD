using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>An append-only audit entry for a <see cref="PickupRequest"/>.</summary>
public class PickupEvent
{
    public Guid Id { get; set; }

    public Guid PickupRequestId { get; set; }
    public PickupRequest PickupRequest { get; set; } = null!;

    public PickupEventType Type { get; set; }

    public PickupRequestStatus? FromStatus { get; set; }
    public PickupRequestStatus? ToStatus { get; set; }

    public string? Note { get; set; }

    public Guid? ActorUserId { get; set; }
    public User? Actor { get; set; }

    public DateTime CreatedAt { get; set; }
}
