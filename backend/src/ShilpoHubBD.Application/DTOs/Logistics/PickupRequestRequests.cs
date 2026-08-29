namespace ShilpoHubBD.Application.DTOs.Logistics;

public class PickupItemRequest
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public string? Reference { get; set; }
    public bool IsFragile { get; set; }
}

public class CreatePickupRequestRequest
{
    /// <summary>Standard, Express or SameDay.</summary>
    public string? Priority { get; set; }

    public Guid? OrderId { get; set; }
    public Guid? OriginProducerUserId { get; set; }

    public string OriginContactName { get; set; } = string.Empty;
    public string OriginPhone { get; set; } = string.Empty;
    public string OriginAddressLine { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public Guid? OriginDistrictId { get; set; }
    public string? OriginPostalCode { get; set; }

    public string? DestinationContactName { get; set; }
    public string? DestinationPhone { get; set; }
    public string? DestinationAddressLine { get; set; }
    public string? DestinationCity { get; set; }
    public Guid? DestinationDistrictId { get; set; }

    public DateTime? ScheduledPickupAt { get; set; }
    public DateTime? PickupWindowEnd { get; set; }

    public int PackageCount { get; set; } = 1;
    public decimal? TotalWeightKg { get; set; }
    public decimal? DeclaredValue { get; set; }
    public bool RequiresColdChain { get; set; }
    public bool IsFragile { get; set; }
    public bool IsCashOnDelivery { get; set; }
    public decimal? CodAmount { get; set; }

    public string? SpecialInstructions { get; set; }

    public List<PickupItemRequest> Items { get; set; } = new();
}

public class UpdatePickupRequestRequest
{
    public string? Priority { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? OriginProducerUserId { get; set; }

    public string? OriginContactName { get; set; }
    public string? OriginPhone { get; set; }
    public string? OriginAddressLine { get; set; }
    public string? OriginCity { get; set; }
    public Guid? OriginDistrictId { get; set; }
    public string? OriginPostalCode { get; set; }

    public string? DestinationContactName { get; set; }
    public string? DestinationPhone { get; set; }
    public string? DestinationAddressLine { get; set; }
    public string? DestinationCity { get; set; }
    public Guid? DestinationDistrictId { get; set; }

    public int? PackageCount { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public decimal? DeclaredValue { get; set; }
    public bool? RequiresColdChain { get; set; }
    public bool? IsFragile { get; set; }
    public bool? IsCashOnDelivery { get; set; }
    public decimal? CodAmount { get; set; }

    public string? SpecialInstructions { get; set; }

    /// <summary>When provided, replaces the full item list.</summary>
    public List<PickupItemRequest>? Items { get; set; }
}

public class SchedulePickupRequestRequest
{
    public DateTime ScheduledPickupAt { get; set; }
    public DateTime? PickupWindowEnd { get; set; }
    public string? Note { get; set; }
}

public class AssignPickupRequestRequest
{
    public string AssignedDriverName { get; set; } = string.Empty;
    public string? AssignedDriverPhone { get; set; }
    public string? AssignedVehicleLabel { get; set; }
    public string? Note { get; set; }
}

public class UpdatePickupStatusRequest
{
    /// <summary>EnRoute, Collected, Completed or Failed.</summary>
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }

    /// <summary>Required when Status = Failed.</summary>
    public string? FailureReason { get; set; }

    /// <summary>Optional override for the actual collection timestamp (defaults to now on Collected).</summary>
    public DateTime? ActualPickupAt { get; set; }
}

public class CancelPickupRequestRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class AddPickupNoteRequest
{
    public string Note { get; set; } = string.Empty;
}

public class PickupRequestQueryParameters
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? OriginProducerUserId { get; set; }
    public Guid? OriginDistrictId { get; set; }
    public DateTime? ScheduledFrom { get; set; }
    public DateTime? ScheduledTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
