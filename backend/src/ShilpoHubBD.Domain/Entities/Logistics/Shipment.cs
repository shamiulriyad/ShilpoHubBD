using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A parcel consignment tracked from collection to delivery, owned by a
/// <see cref="LogisticsPartnerProfile"/>. May reference the marketplace <see cref="Order"/> it
/// fulfils, the <see cref="PickupRequest"/> it came from and the <see cref="DeliveryRoute"/> currently
/// carrying it. The append-only <see cref="Events"/> list is the customer-facing tracking timeline.
/// </summary>
public class Shipment
{
    public Guid Id { get; set; }

    /// <summary>Public tracking reference, format <c>SHP-yyyyMM-#####</c>. Unique.</summary>
    public string TrackingNumber { get; set; } = string.Empty;

    public Guid LogisticsPartnerProfileId { get; set; }
    public LogisticsPartnerProfile Profile { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;
    public ShipmentServiceLevel ServiceLevel { get; set; } = ShipmentServiceLevel.Standard;

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid? PickupRequestId { get; set; }
    public PickupRequest? PickupRequest { get; set; }

    public Guid? DeliveryRouteId { get; set; }
    public DeliveryRoute? DeliveryRoute { get; set; }

    // ---- Origin -----------------------------------------------------------
    public string OriginContactName { get; set; } = string.Empty;
    public string OriginPhone { get; set; } = string.Empty;
    public string OriginAddressLine { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public Guid? OriginDistrictId { get; set; }
    public District? OriginDistrict { get; set; }
    public string? OriginPostalCode { get; set; }

    // ---- Destination -------------------------------------------------
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string DestinationAddressLine { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public Guid? DestinationDistrictId { get; set; }
    public District? DestinationDistrict { get; set; }
    public string? DestinationPostalCode { get; set; }

    // ---- Consignment ----------------------------------------------
    public int ParcelCount { get; set; } = 1;
    public decimal? TotalWeightKg { get; set; }
    public string? DimensionsNote { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? ShippingCost { get; set; }

    public bool IsCashOnDelivery { get; set; }
    public decimal? CodAmount { get; set; }
    public bool CodCollected { get; set; }
    public DateTime? CodCollectedAt { get; set; }

    // ---- Live position -------------------------------------------
    public string? CurrentLocationLabel { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }

    // ---- Milestones -------------------------------------------
    public DateTime? EstimatedDeliveryAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? LastStatusAt { get; set; }
    public int DeliveryAttemptCount { get; set; }

    // ---- Proof of delivery -----------------------------------
    public string? ReceivedByName { get; set; }
    public string? ProofOfDeliveryNote { get; set; }
    public string? SignatureImageUrl { get; set; }

    public string? FailureReason { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ShipmentTrackingEvent> Events { get; set; } = new List<ShipmentTrackingEvent>();
    public ICollection<DeliveryAttempt> Attempts { get; set; } = new List<DeliveryAttempt>();
}
