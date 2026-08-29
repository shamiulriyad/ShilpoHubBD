using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>A recorded attempt to hand a <see cref="Shipment"/> to its recipient.</summary>
public class DeliveryAttempt
{
    public Guid Id { get; set; }

    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public int AttemptNumber { get; set; }
    public DeliveryAttemptOutcome Outcome { get; set; }

    public DateTime AttemptedAt { get; set; }
    public string? Note { get; set; }
    public DateTime? NextAttemptAt { get; set; }

    public Guid? RecordedByUserId { get; set; }
    public User? RecordedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}
