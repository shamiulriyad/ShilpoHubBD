namespace ShilpoHubBD.Application.DTOs.Logistics;

public class ReturnItemInput
{
    public Guid? ProductId { get; set; }
    public string? Sku { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal? UnitRefundAmount { get; set; }
    public string? Notes { get; set; }
}

public class CreateReturnRequestRequest
{
    public Guid? ShipmentId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }

    /// <summary>DamagedInTransit, DefectiveProduct, WrongItem, NotAsDescribed, CustomerChangedMind,
    /// DeliveryFailed, Undeliverable, LateDelivery or Other.</summary>
    public string Reason { get; set; } = string.Empty;
    public string? ReasonDetail { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public string? PickupContactName { get; set; }
    public string? PickupPhone { get; set; }
    public string? PickupAddressLine { get; set; }
    public string? PickupCity { get; set; }
    public Guid? PickupDistrictId { get; set; }
    public string? PickupPostalCode { get; set; }

    public List<ReturnItemInput> Items { get; set; } = new();
}

public class UpdateReturnRequestRequest
{
    public string? Reason { get; set; }
    public string? ReasonDetail { get; set; }
    public Guid? ShipmentId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? PickupContactName { get; set; }
    public string? PickupPhone { get; set; }
    public string? PickupAddressLine { get; set; }
    public string? PickupCity { get; set; }
    public Guid? PickupDistrictId { get; set; }
    public string? PickupPostalCode { get; set; }

    /// <summary>When provided, replaces the full item list. Only allowed before approval.</summary>
    public List<ReturnItemInput>? Items { get; set; }
}

public class ApproveReturnRequestRequest
{
    public Guid? DestinationWarehouseId { get; set; }
    public string? Note { get; set; }
}

public class RejectReturnRequestRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ScheduleReturnPickupRequest
{
    public DateTime ScheduledPickupAt { get; set; }
    public string? PickupContactName { get; set; }
    public string? PickupPhone { get; set; }
    public string? PickupAddressLine { get; set; }
    public string? PickupCity { get; set; }
    public Guid? PickupDistrictId { get; set; }
    public string? PickupPostalCode { get; set; }
    public string? AssignedCarrierLabel { get; set; }
    public string? AssignedDriverName { get; set; }
    public string? Note { get; set; }
}

public class UpdateReturnStatusRequest
{
    /// <summary>InTransit, Received, UnderInspection or Closed.</summary>
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }

    /// <summary>Optional override of the receipt timestamp when moving to Received.</summary>
    public DateTime? ReceivedAt { get; set; }
}

public class ReturnItemAssessmentInput
{
    public Guid ReturnItemId { get; set; }
    public int? QuantityReceived { get; set; }

    /// <summary>NotReceived, New, LikeNew, Used, Damaged, Defective or Unsalvageable.</summary>
    public string? Condition { get; set; }

    /// <summary>Pending, Restock, ReturnToProducer, Repair, Refurbish, Scrap or Donate.</summary>
    public string? Disposition { get; set; }

    public decimal? UnitRefundAmount { get; set; }
    public string? Notes { get; set; }
}

public class RecordReturnInspectionRequest
{
    /// <summary>NotReceived, New, LikeNew, Used, Damaged, Defective or Unsalvageable.</summary>
    public string OverallCondition { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    /// <summary>Refund, Replacement, Repair, StoreCredit or NoAction.</summary>
    public string RecommendedResolution { get; set; } = string.Empty;

    public string? PhotosJson { get; set; }
    public DateTime? InspectedAt { get; set; }

    public List<ReturnItemAssessmentInput> ItemAssessments { get; set; } = new();
}

public class RestockReturnRequest
{
    public Guid? DestinationWarehouseId { get; set; }
    public string? Note { get; set; }

    /// <summary>Per-item restocked quantities. Items omitted keep their current value.</summary>
    public List<RestockReturnItemInput> Items { get; set; } = new();
}

public class RestockReturnItemInput
{
    public Guid ReturnItemId { get; set; }
    public int RestockedQuantity { get; set; }
}

public class RecordReturnRefundRequest
{
    public decimal RefundAmount { get; set; }

    /// <summary>Refund, Replacement, Repair, StoreCredit or NoAction.</summary>
    public string? ResolutionType { get; set; }
    public string? ResolutionNote { get; set; }
    public string? RefundMethod { get; set; }
    public string? RefundReference { get; set; }

    /// <summary>Mark the refund as already paid (status Refunded) instead of RefundPending.</summary>
    public bool MarkPaid { get; set; }
    public DateTime? RefundedAt { get; set; }
}

public class CancelReturnRequestRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CloseReturnRequestRequest
{
    public string? Note { get; set; }
}

public class AddReturnNoteRequest
{
    public string Note { get; set; } = string.Empty;
}

public class ReturnRequestQueryParameters
{
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public Guid? ShipmentId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
