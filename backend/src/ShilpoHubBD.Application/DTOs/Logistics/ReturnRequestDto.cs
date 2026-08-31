namespace ShilpoHubBD.Application.DTOs.Logistics;

public class ReturnRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;

    public Guid LogisticsPartnerProfileId { get; set; }
    public string? LogisticsPartnerName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }

    public Guid? ShipmentId { get; set; }
    public string? ShipmentTrackingNumber { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public string? DestinationWarehouseCode { get; set; }

    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? ReasonDetail { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public string? PickupContactName { get; set; }
    public string? PickupPhone { get; set; }
    public string? PickupAddressLine { get; set; }
    public string? PickupCity { get; set; }
    public Guid? PickupDistrictId { get; set; }
    public string? PickupDistrictName { get; set; }
    public string? PickupPostalCode { get; set; }
    public DateTime? ScheduledPickupAt { get; set; }
    public DateTime? ActualPickupAt { get; set; }
    public string? AssignedCarrierLabel { get; set; }
    public string? AssignedDriverName { get; set; }
    public DateTime? ReceivedAt { get; set; }

    public string? ResolutionType { get; set; }
    public string? ResolutionNote { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundMethod { get; set; }
    public string? RefundReference { get; set; }
    public DateTime? RefundedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ReturnItemDto> Items { get; set; } = new();
    public List<ReturnInspectionDto> Inspections { get; set; } = new();
    public List<ReturnEventDto> Events { get; set; } = new();
}

public class ReturnRequestListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? PickupCity { get; set; }
    public DateTime? ScheduledPickupAt { get; set; }
    public decimal? RefundAmount { get; set; }
    public int ItemCount { get; set; }
    public Guid? ShipmentId { get; set; }
    public Guid? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReturnItemDto
{
    public Guid Id { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? Sku { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int QuantityReceived { get; set; }
    public int RestockedQuantity { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Disposition { get; set; } = string.Empty;
    public decimal? UnitRefundAmount { get; set; }
    public string? Notes { get; set; }
}

public class ReturnInspectionDto
{
    public Guid Id { get; set; }
    public Guid? InspectedByUserId { get; set; }
    public string? InspectedByName { get; set; }
    public DateTime InspectedAt { get; set; }
    public string OverallCondition { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string RecommendedResolution { get; set; } = string.Empty;
    public string? PhotosJson { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReturnEventDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? Note { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public DateTime CreatedAt { get; set; }
}
