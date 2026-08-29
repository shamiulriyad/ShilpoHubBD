namespace ShilpoHubBD.Application.DTOs.Logistics;

public class DeliveryRouteDto
{
    public Guid Id { get; set; }
    public string RouteCode { get; set; } = string.Empty;

    public Guid LogisticsPartnerProfileId { get; set; }
    public string? LogisticsPartnerName { get; set; }

    public Guid CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public DateTime? ScheduledDate { get; set; }
    public DateTime? PlannedStartAt { get; set; }
    public DateTime? PlannedEndAt { get; set; }
    public DateTime? ActualStartAt { get; set; }
    public DateTime? ActualEndAt { get; set; }

    public string? StartLocationLabel { get; set; }
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }
    public string? EndLocationLabel { get; set; }
    public double? EndLatitude { get; set; }
    public double? EndLongitude { get; set; }

    public Guid? OriginDistrictId { get; set; }
    public string? OriginDistrictName { get; set; }

    public string? AssignedDriverName { get; set; }
    public string? AssignedDriverPhone { get; set; }
    public string? AssignedVehicleLabel { get; set; }
    public decimal? VehicleCapacityKg { get; set; }
    public DateTime? AssignedAt { get; set; }

    public int TotalStops { get; set; }
    public int CompletedStops { get; set; }
    public decimal TotalLoadKg { get; set; }
    public decimal? TotalDistanceKm { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public string OptimizationStrategy { get; set; } = "manual";

    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<RouteStopDto> Stops { get; set; } = new();
    public List<RouteEventDto> Events { get; set; } = new();
}

public class DeliveryRouteListItemDto
{
    public Guid Id { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public string? AssignedDriverName { get; set; }
    public string? AssignedVehicleLabel { get; set; }
    public int TotalStops { get; set; }
    public int CompletedStops { get; set; }
    public decimal TotalLoadKg { get; set; }
    public decimal? TotalDistanceKm { get; set; }
    public string OptimizationStrategy { get; set; } = "manual";
    public DateTime CreatedAt { get; set; }
}

public class RouteStopDto
{
    public Guid Id { get; set; }
    public int Sequence { get; set; }
    public string StopType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Guid? PickupRequestId { get; set; }
    public string? PickupReferenceCode { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }

    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public decimal? LoadKg { get; set; }
    public int PackageCount { get; set; }

    public DateTime? PlannedArrivalAt { get; set; }
    public DateTime? PlannedDepartureAt { get; set; }
    public DateTime? ActualArrivalAt { get; set; }
    public DateTime? ActualDepartureAt { get; set; }
    public int? ServiceDurationMinutes { get; set; }
    public decimal? DistanceFromPreviousKm { get; set; }

    public string? Instructions { get; set; }
    public string? CompletionNote { get; set; }
    public string? FailureReason { get; set; }
}

public class RouteEventDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? RouteStopId { get; set; }
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? Note { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public DateTime CreatedAt { get; set; }
}
