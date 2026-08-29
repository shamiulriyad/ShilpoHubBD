using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A request to bring goods back from a customer, handled by a <see cref="LogisticsPartnerProfile"/>.
/// Covers the whole flow: approval, reverse pickup, receipt, inspection, disposition / restock and
/// refund. May reference the originating <see cref="Shipment"/> / <see cref="Order"/> and the
/// <see cref="Warehouse"/> the goods come back to.
/// </summary>
public class ReturnRequest
{
    public Guid Id { get; set; }

    /// <summary>Human reference, format <c>RTN-yyyyMM-#####</c>. Unique.</summary>
    public string ReferenceCode { get; set; } = string.Empty;

    public Guid LogisticsPartnerProfileId { get; set; }
    public LogisticsPartnerProfile Profile { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public Guid? ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid? DestinationWarehouseId { get; set; }
    public Warehouse? DestinationWarehouse { get; set; }

    public ReturnStatus Status { get; set; } = ReturnStatus.Requested;
    public ReturnReason Reason { get; set; }
    public string? ReasonDetail { get; set; }

    // ---- Customer (snapshot) -------------------------------------------
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // ---- Reverse pickup ---------------------------------------------
    public string? PickupContactName { get; set; }
    public string? PickupPhone { get; set; }
    public string? PickupAddressLine { get; set; }
    public string? PickupCity { get; set; }
    public Guid? PickupDistrictId { get; set; }
    public District? PickupDistrict { get; set; }
    public string? PickupPostalCode { get; set; }

    public DateTime? ScheduledPickupAt { get; set; }
    public DateTime? ActualPickupAt { get; set; }
    public string? AssignedCarrierLabel { get; set; }
    public string? AssignedDriverName { get; set; }

    public DateTime? ReceivedAt { get; set; }

    // ---- Resolution / refund ---------------------------------
    public ReturnResolutionType? ResolutionType { get; set; }
    public string? ResolutionNote { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundMethod { get; set; }
    public string? RefundReference { get; set; }
    public DateTime? RefundedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ReturnItem> Items { get; set; } = new List<ReturnItem>();
    public ICollection<ReturnInspection> Inspections { get; set; } = new List<ReturnInspection>();
    public ICollection<ReturnEvent> Events { get; set; } = new List<ReturnEvent>();
}
