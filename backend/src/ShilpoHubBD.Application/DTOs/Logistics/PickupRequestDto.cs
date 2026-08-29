namespace ShilpoHubBD.Application.DTOs.Logistics;

public class PickupRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;

    public Guid LogisticsPartnerProfileId { get; set; }
    public string? LogisticsPartnerName { get; set; }

    public Guid RequestedByUserId { get; set; }
    public string? RequestedByName { get; set; }

    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }

    public string OriginContactName { get; set; } = string.Empty;
    public string OriginPhone { get; set; } = string.Empty;
    public string OriginAddressLine { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public Guid? OriginDistrictId { get; set; }
    public string? OriginDistrictName { get; set; }
    public string? OriginPostalCode { get; set; }
    public Guid? OriginProducerUserId { get; set; }
    public string? OriginProducerName { get; set; }

    public string? DestinationContactName { get; set; }
    public string? DestinationPhone { get; set; }
    public string? DestinationAddressLine { get; set; }
    public string? DestinationCity { get; set; }
    public Guid? DestinationDistrictId { get; set; }
    public string? DestinationDistrictName { get; set; }

    public DateTime? ScheduledPickupAt { get; set; }
    public DateTime? PickupWindowEnd { get; set; }
    public DateTime? ActualPickupAt { get; set; }

    public int PackageCount { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public decimal? DeclaredValue { get; set; }
    public bool RequiresColdChain { get; set; }
    public bool IsFragile { get; set; }
    public bool IsCashOnDelivery { get; set; }
    public decimal? CodAmount { get; set; }

    public string? AssignedDriverName { get; set; }
    public string? AssignedDriverPhone { get; set; }
    public string? AssignedVehicleLabel { get; set; }
    public DateTime? AssignedAt { get; set; }

    public string? SpecialInstructions { get; set; }
    public string? CancellationReason { get; set; }
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<PickupItemDto> Items { get; set; } = new();
    public List<PickupEventDto> Events { get; set; } = new();
}

public class PickupRequestListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string? OriginDistrictName { get; set; }
    public string? DestinationCity { get; set; }
    public DateTime? ScheduledPickupAt { get; set; }
    public int PackageCount { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public string? AssignedDriverName { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PickupItemDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public string? Reference { get; set; }
    public bool IsFragile { get; set; }
}

public class PickupEventDto
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
