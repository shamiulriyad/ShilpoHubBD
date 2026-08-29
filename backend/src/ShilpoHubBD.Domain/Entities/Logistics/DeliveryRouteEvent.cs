using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>An append-only audit entry for a <see cref="DeliveryRoute"/>.</summary>
public class DeliveryRouteEvent
{
    public Guid Id { get; set; }

    public Guid DeliveryRouteId { get; set; }
    public DeliveryRoute Route { get; set; } = null!;

    public DeliveryRouteEventType Type { get; set; }

    /// <summary>The stop this event concerns, when applicable. Not a FK — stops may be removed.</summary>
    public Guid? RouteStopId { get; set; }

    public DeliveryRouteStatus? FromStatus { get; set; }
    public DeliveryRouteStatus? ToStatus { get; set; }

    public string? Note { get; set; }

    public Guid? ActorUserId { get; set; }
    public User? Actor { get; set; }

    public DateTime CreatedAt { get; set; }
}
