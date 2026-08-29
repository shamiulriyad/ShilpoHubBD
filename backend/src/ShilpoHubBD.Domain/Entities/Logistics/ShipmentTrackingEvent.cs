using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>One entry on a <see cref="Shipment"/>'s tracking timeline (append-only).</summary>
public class ShipmentTrackingEvent
{
    public Guid Id { get; set; }

    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public ShipmentEventType EventType { get; set; }

    public ShipmentStatus? FromStatus { get; set; }
    public ShipmentStatus? ToStatus { get; set; }

    public string? LocationLabel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

    public string? Description { get; set; }

    /// <summary>When the event physically occurred (may pre-date <see cref="CreatedAt"/>).</summary>
    public DateTime OccurredAt { get; set; }

    public Guid? RecordedByUserId { get; set; }
    public User? RecordedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}
