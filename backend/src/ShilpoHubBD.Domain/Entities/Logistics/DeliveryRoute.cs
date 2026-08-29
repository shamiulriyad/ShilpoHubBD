using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A planned run of stops (pickups and/or deliveries) for one crew / vehicle on a given day, owned by
/// a <see cref="LogisticsPartnerProfile"/>. Stops are sequenced manually or by the built-in
/// nearest-neighbour optimiser; the AI route optimiser is a later part.
/// </summary>
public class DeliveryRoute
{
    public Guid Id { get; set; }

    /// <summary>Human reference, format <c>RT-yyyyMM-#####</c>. Unique.</summary>
    public string RouteCode { get; set; } = string.Empty;

    public Guid LogisticsPartnerProfileId { get; set; }
    public LogisticsPartnerProfile Profile { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public DeliveryRouteStatus Status { get; set; } = DeliveryRouteStatus.Draft;

    public DateTime? ScheduledDate { get; set; }
    public DateTime? PlannedStartAt { get; set; }
    public DateTime? PlannedEndAt { get; set; }
    public DateTime? ActualStartAt { get; set; }
    public DateTime? ActualEndAt { get; set; }

    // ---- Start / end anchor ------------------------------------------
    public string? StartLocationLabel { get; set; }
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }
    public string? EndLocationLabel { get; set; }
    public double? EndLatitude { get; set; }
    public double? EndLongitude { get; set; }

    public Guid? OriginDistrictId { get; set; }
    public District? OriginDistrict { get; set; }

    // ---- Crew / vehicle --------------------------------------------
    public string? AssignedDriverName { get; set; }
    public string? AssignedDriverPhone { get; set; }
    public string? AssignedVehicleLabel { get; set; }
    public decimal? VehicleCapacityKg { get; set; }
    public DateTime? AssignedAt { get; set; }

    // ---- Roll-ups (maintained on write) --------------------------
    public int TotalStops { get; set; }
    public int CompletedStops { get; set; }
    public decimal TotalLoadKg { get; set; }
    public decimal? TotalDistanceKm { get; set; }
    public int? EstimatedDurationMinutes { get; set; }

    /// <summary>How the current stop order was produced, e.g. <c>manual</c> or <c>nearest-neighbor</c>.</summary>
    public string OptimizationStrategy { get; set; } = "manual";

    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<DeliveryRouteStop> Stops { get; set; } = new List<DeliveryRouteStop>();
    public ICollection<DeliveryRouteEvent> Events { get; set; } = new List<DeliveryRouteEvent>();
}
