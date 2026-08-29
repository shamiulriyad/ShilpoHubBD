namespace ShilpoHubBD.Application.DTOs.Logistics;

public class ShipmentDto
{
    public Guid Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;

    public Guid LogisticsPartnerProfileId { get; set; }
    public string? LogisticsPartnerName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }

    public string Status { get; set; } = string.Empty;
    public string ServiceLevel { get; set; } = string.Empty;

    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? PickupRequestId { get; set; }
    public string? PickupReferenceCode { get; set; }
    public Guid? DeliveryRouteId { get; set; }
    public string? DeliveryRouteCode { get; set; }

    public string OriginContactName { get; set; } = string.Empty;
    public string OriginPhone { get; set; } = string.Empty;
    public string OriginAddressLine { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public Guid? OriginDistrictId { get; set; }
    public string? OriginDistrictName { get; set; }
    public string? OriginPostalCode { get; set; }

    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string DestinationAddressLine { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public Guid? DestinationDistrictId { get; set; }
    public string? DestinationDistrictName { get; set; }
    public string? DestinationPostalCode { get; set; }

    public int ParcelCount { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public string? DimensionsNote { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? ShippingCost { get; set; }

    public bool IsCashOnDelivery { get; set; }
    public decimal? CodAmount { get; set; }
    public bool CodCollected { get; set; }
    public DateTime? CodCollectedAt { get; set; }

    public string? CurrentLocationLabel { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }

    public DateTime? EstimatedDeliveryAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? LastStatusAt { get; set; }
    public int DeliveryAttemptCount { get; set; }

    public string? ReceivedByName { get; set; }
    public string? ProofOfDeliveryNote { get; set; }
    public string? SignatureImageUrl { get; set; }
    public string? FailureReason { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ShipmentTrackingEventDto> Events { get; set; } = new();
    public List<DeliveryAttemptDto> Attempts { get; set; } = new();
}

public class ShipmentListItemDto
{
    public Guid Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ServiceLevel { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public string? DestinationDistrictName { get; set; }
    public int ParcelCount { get; set; }
    public bool IsCashOnDelivery { get; set; }
    public DateTime? EstimatedDeliveryAt { get; set; }
    public DateTime? LastStatusAt { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ShipmentTrackingEventDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? LocationLabel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? Description { get; set; }
    public DateTime OccurredAt { get; set; }
    public Guid? RecordedByUserId { get; set; }
    public string? RecordedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DeliveryAttemptDto
{
    public Guid Id { get; set; }
    public int AttemptNumber { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public DateTime AttemptedAt { get; set; }
    public string? Note { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public Guid? RecordedByUserId { get; set; }
    public string? RecordedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Lean, PII-light projection returned by the public tracking lookup.</summary>
public class ShipmentTrackingDto
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ServiceLevel { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public string? DestinationDistrictName { get; set; }
    public int ParcelCount { get; set; }
    public DateTime? EstimatedDeliveryAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? LastStatusAt { get; set; }
    public string? CurrentLocationLabel { get; set; }
    public List<ShipmentTrackingCheckpointDto> Checkpoints { get; set; } = new();
}

public class ShipmentTrackingCheckpointDto
{
    public string EventType { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? LocationLabel { get; set; }
    public string? DistrictName { get; set; }
    public string? Description { get; set; }
    public DateTime OccurredAt { get; set; }
}
