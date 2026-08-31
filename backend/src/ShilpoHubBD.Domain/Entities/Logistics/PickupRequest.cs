using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A request for a logistics partner to collect goods from an origin (typically a producer) so they
/// can enter the delivery pipeline. Owned by a <see cref="LogisticsPartnerProfile"/>; may optionally
/// reference a marketplace <see cref="Order"/> it is fulfilling.
/// </summary>
public class PickupRequest
{
    public Guid Id { get; set; }

    /// <summary>Human reference, format <c>PU-yyyyMM-#####</c>. Unique.</summary>
    public string ReferenceCode { get; set; } = string.Empty;

    public Guid LogisticsPartnerProfileId { get; set; }
    public LogisticsPartnerProfile Profile { get; set; } = null!;

    public Guid RequestedByUserId { get; set; }
    public User RequestedBy { get; set; } = null!;

    public PickupRequestStatus Status { get; set; } = PickupRequestStatus.Draft;
    public PickupPriority Priority { get; set; } = PickupPriority.Standard;

    /// <summary>Optional marketplace order this pickup fulfils.</summary>
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    // ---- Origin --------------------------------------------------------
    public string OriginContactName { get; set; } = string.Empty;
    public string OriginPhone { get; set; } = string.Empty;
    public string OriginAddressLine { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public Guid? OriginDistrictId { get; set; }
    public District? OriginDistrict { get; set; }
    public string? OriginPostalCode { get; set; }

    /// <summary>Producer being collected from, when known.</summary>
    public Guid? OriginProducerUserId { get; set; }
    public User? OriginProducer { get; set; }

    // ---- Destination (optional at pickup time) -----------------------
    public string? DestinationContactName { get; set; }
    public string? DestinationPhone { get; set; }
    public string? DestinationAddressLine { get; set; }
    public string? DestinationCity { get; set; }
    public Guid? DestinationDistrictId { get; set; }
    public District? DestinationDistrict { get; set; }

    // ---- Schedule -------------------------------------------------
    public DateTime? ScheduledPickupAt { get; set; }
    public DateTime? PickupWindowEnd { get; set; }
    public DateTime? ActualPickupAt { get; set; }

    // ---- Consignment -------------------------------------------
    public int PackageCount { get; set; } = 1;
    public decimal? TotalWeightKg { get; set; }
    public decimal? DeclaredValue { get; set; }
    public bool RequiresColdChain { get; set; }
    public bool IsFragile { get; set; }
    public bool IsCashOnDelivery { get; set; }
    public decimal? CodAmount { get; set; }

    // ---- Assignment ------------------------------------------
    public string? AssignedDriverName { get; set; }
    public string? AssignedDriverPhone { get; set; }
    public string? AssignedVehicleLabel { get; set; }
    public DateTime? AssignedAt { get; set; }

    public string? SpecialInstructions { get; set; }
    public string? CancellationReason { get; set; }
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PickupItem> Items { get; set; } = new List<PickupItem>();
    public ICollection<PickupEvent> Events { get; set; } = new List<PickupEvent>();
}
