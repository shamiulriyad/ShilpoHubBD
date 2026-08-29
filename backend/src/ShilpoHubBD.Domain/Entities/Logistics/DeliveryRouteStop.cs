using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// One stop on a <see cref="DeliveryRoute"/>. May reference a Part-1 <see cref="PickupRequest"/> or a
/// marketplace <see cref="Order"/> it services, or stand alone (transfer / waypoint).
/// </summary>
public class DeliveryRouteStop
{
    public Guid Id { get; set; }

    public Guid DeliveryRouteId { get; set; }
    public DeliveryRoute Route { get; set; } = null!;

    /// <summary>1-based position in the route.</summary>
    public int Sequence { get; set; }

    public DeliveryRouteStopType StopType { get; set; }
    public DeliveryRouteStopStatus Status { get; set; } = DeliveryRouteStopStatus.Pending;

    public Guid? PickupRequestId { get; set; }
    public PickupRequest? PickupRequest { get; set; }

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }

    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Guid? DistrictId { get; set; }
    public District? District { get; set; }
    public string? PostalCode { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    /// <summary>Weight loaded (pickup) or dropped (delivery) at this stop, kg.</summary>
    public decimal? LoadKg { get; set; }
    public int PackageCount { get; set; } = 1;

    public DateTime? PlannedArrivalAt { get; set; }
    public DateTime? PlannedDepartureAt { get; set; }
    public DateTime? ActualArrivalAt { get; set; }
    public DateTime? ActualDepartureAt { get; set; }

    /// <summary>Expected dwell time at the stop, minutes.</summary>
    public int? ServiceDurationMinutes { get; set; }

    /// <summary>Leg distance from the previous stop (or route start), km. Set by the optimiser.</summary>
    public decimal? DistanceFromPreviousKm { get; set; }

    public string? Instructions { get; set; }
    public string? CompletionNote { get; set; }
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
