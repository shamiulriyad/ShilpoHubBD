using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A rule-based re-sequencing proposal for a <see cref="DeliveryRoute"/> (nearest-neighbour seed plus
/// a 2-opt improvement pass over haversine legs), produced by the pluggable AI route-optimisation
/// provider. Applying a run writes the proposed order back onto the route's stops. No real model.
/// </summary>
public class RouteOptimizationRun
{
    public Guid Id { get; set; }

    public Guid LogisticsPartnerProfileId { get; set; }
    public LogisticsPartnerProfile Profile { get; set; } = null!;

    public Guid DeliveryRouteId { get; set; }
    public DeliveryRoute DeliveryRoute { get; set; } = null!;

    public Guid GeneratedByUserId { get; set; }
    public User GeneratedBy { get; set; } = null!;

    public RouteOptimizationRunStatus Status { get; set; } = RouteOptimizationRunStatus.Proposed;

    public string Method { get; set; } = string.Empty;
    public string Objective { get; set; } = "proximity";
    public string Summary { get; set; } = string.Empty;

    public decimal? OriginalDistanceKm { get; set; }
    public decimal? ProposedDistanceKm { get; set; }
    public decimal? DistanceSavingKm { get; set; }
    public int? ProposedDurationMinutes { get; set; }
    public AiLogisticsConfidence Confidence { get; set; }

    public DateTime? AppliedAt { get; set; }
    public Guid? AppliedByUserId { get; set; }
    public User? AppliedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<RouteOptimizationRunStop> Stops { get; set; } = new List<RouteOptimizationRunStop>();
}
