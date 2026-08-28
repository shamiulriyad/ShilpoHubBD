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

    public List<ProcurementItemDto> Items { get; set; } = new();
    public List<ProcurementStatusEventDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
