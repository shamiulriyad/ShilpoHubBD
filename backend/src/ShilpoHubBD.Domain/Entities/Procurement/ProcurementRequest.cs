using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Domain.Entities.Procurement;

public class ProcurementRequest
{
    public Guid Id { get; set; }

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal? Budget { get; set; }
    public DateTime DeliveryDeadline { get; set; }

    public ProcurementStatus Status { get; set; } = ProcurementStatus.PendingApproval;

    // Optional link back to the quotation this procurement was converted from.
    public Guid? QuotationRequestId { get; set; }
    public QuotationRequest? QuotationRequestRef { get; set; }
    public Guid? QuotationResponseId { get; set; }
    public QuotationResponse? QuotationResponseRef { get; set; }

    // Set once the procurement is converted into a real Order (reuses the existing Order module).
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }

    // ---- Advance payment: the partner pays at least 50% of the total before the deal goes ahead
    public decimal? AdvanceAmount { get; set; }
    public DateTime? AdvancePaidAt { get; set; }
    public string? AdvanceMethod { get; set; }
    public string? AdvanceReference { get; set; }
    public DateTime? AdvanceRefundedAt { get; set; }

    // ---- Admin inspection after the advance is paid
    public ProcurementInspectionStatus InspectionStatus { get; set; } = ProcurementInspectionStatus.NotRequired;
    public Guid? InspectedByUserId { get; set; }
    public User? InspectedBy { get; set; }
    public DateTime? InspectedAt { get; set; }
    public string? InspectionNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProcurementItem> Items { get; set; } = new List<ProcurementItem>();
    public ICollection<ProcurementStatusEvent> StatusHistory { get; set; } = new List<ProcurementStatusEvent>();
}
