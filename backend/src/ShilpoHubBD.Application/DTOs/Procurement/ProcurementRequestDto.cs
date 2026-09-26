using ShilpoHubBD.Domain.Entities.Procurement;

namespace ShilpoHubBD.Application.DTOs.Procurement;

public class ProcurementRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public decimal? Budget { get; set; }
    public decimal ItemsTotal { get; set; }
    public DateTime DeliveryDeadline { get; set; }
    public ProcurementStatus Status { get; set; }

    public Guid? QuotationRequestId { get; set; }
    public Guid? QuotationResponseId { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }

    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }

    // Advance payment (at least 50% of ItemsTotal) and the admin inspection that follows.
    public decimal RequiredAdvance { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public DateTime? AdvancePaidAt { get; set; }
    public string? AdvanceMethod { get; set; }
    public string? AdvanceReference { get; set; }
    public DateTime? AdvanceRefundedAt { get; set; }
    public ProcurementInspectionStatus InspectionStatus { get; set; }
    public string? InspectedByName { get; set; }
    public DateTime? InspectedAt { get; set; }
    public string? InspectionNotes { get; set; }

    public List<ProcurementItemDto> Items { get; set; } = new();
    public List<ProcurementStatusEventDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
