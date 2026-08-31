namespace ShilpoHubBD.Application.DTOs.Logistics;

public class CreateShipmentRequest
{
    /// <summary>Economy, Standard, Express or SameDay. Defaults to Standard.</summary>
    public string? ServiceLevel { get; set; }

    public Guid? OrderId { get; set; }
    public Guid? PickupRequestId { get; set; }
    public Guid? DeliveryRouteId { get; set; }

    public string OriginContactName { get; set; } = string.Empty;
    public string OriginPhone { get; set; } = string.Empty;
    public string OriginAddressLine { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public Guid? OriginDistrictId { get; set; }
    public string? OriginPostalCode { get; set; }

    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string DestinationAddressLine { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public Guid? DestinationDistrictId { get; set; }
    public string? DestinationPostalCode { get; set; }

    public int ParcelCount { get; set; } = 1;
    public decimal? TotalWeightKg { get; set; }
    public string? DimensionsNote { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? ShippingCost { get; set; }

    public bool IsCashOnDelivery { get; set; }
    public decimal? CodAmount { get; set; }

    public DateTime? EstimatedDeliveryAt { get; set; }

    /// <summary>Create the shipment already at LabelCreated instead of Created.</summary>
    public bool LabelCreated { get; set; }
}

public class UpdateShipmentRequest
{
    public string? ServiceLevel { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? PickupRequestId { get; set; }
    public Guid? DeliveryRouteId { get; set; }

    public string? OriginContactName { get; set; }
    public string? OriginPhone { get; set; }
    public string? OriginAddressLine { get; set; }
    public string? OriginCity { get; set; }
    public Guid? OriginDistrictId { get; set; }
    public string? OriginPostalCode { get; set; }

    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }
    public string? DestinationAddressLine { get; set; }
    public string? DestinationCity { get; set; }
    public Guid? DestinationDistrictId { get; set; }
    public string? DestinationPostalCode { get; set; }

    public int? ParcelCount { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public string? DimensionsNote { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? ShippingCost { get; set; }
    public bool? IsCashOnDelivery { get; set; }
    public decimal? CodAmount { get; set; }
    public DateTime? EstimatedDeliveryAt { get; set; }
}

public class UpdateShipmentStatusRequest
{
    /// <summary>Target status: LabelCreated, PickedUp, InTransit, AtHub, OutForDelivery, DeliveryFailed, Returned.</summary>
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LocationLabel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid? DistrictId { get; set; }
    public DateTime? OccurredAt { get; set; }

    /// <summary>Required when Status = DeliveryFailed.</summary>
    public string? FailureReason { get; set; }
}

public class AddShipmentTrackingEventRequest
{
    /// <summary>LocationUpdated, ArrivedAtHub, DepartedHub, OutForDelivery, Exception or NoteAdded.</summary>
    public string EventType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LocationLabel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid? DistrictId { get; set; }
    public DateTime? OccurredAt { get; set; }

    /// <summary>Also set the shipment's live position from this event's location.</summary>
    public bool UpdateCurrentLocation { get; set; } = true;
}

public class UpdateShipmentLocationRequest
{
    public string LocationLabel { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid? DistrictId { get; set; }
    public DateTime? OccurredAt { get; set; }
}

public class RecordDeliveryAttemptRequest
{
    /// <summary>Delivered, RecipientUnavailable, AddressNotFound, Refused, Rescheduled, Damaged or Other.</summary>
    public string Outcome { get; set; } = string.Empty;
    public DateTime? AttemptedAt { get; set; }
    public string? Note { get; set; }
    public DateTime? NextAttemptAt { get; set; }

    // Used only when Outcome = Delivered.
    public string? ReceivedByName { get; set; }
    public string? ProofOfDeliveryNote { get; set; }
    public string? SignatureImageUrl { get; set; }
    public bool CodCollected { get; set; }
}

public class MarkShipmentDeliveredRequest
{
    public string? ReceivedByName { get; set; }
    public string? ProofOfDeliveryNote { get; set; }
    public string? SignatureImageUrl { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool CodCollected { get; set; }
}

public class CancelShipmentRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class AddShipmentNoteRequest
{
    public string Note { get; set; } = string.Empty;
}

public class ShipmentQueryParameters
{
    public string? Status { get; set; }
    public string? ServiceLevel { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? DeliveryRouteId { get; set; }
    public Guid? DestinationDistrictId { get; set; }
    public bool? IsCashOnDelivery { get; set; }
    public bool? Overdue { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
