namespace ShilpoHubBD.Application.DTOs.Logistics;

public class RouteStopInput
{
    /// <summary>Pickup, Delivery, Transfer or Waypoint.</summary>
    public string StopType { get; set; } = string.Empty;

    public Guid? PickupRequestId { get; set; }
    public Guid? OrderId { get; set; }

    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Guid? DistrictId { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public decimal? LoadKg { get; set; }
    public int PackageCount { get; set; } = 1;

    public DateTime? PlannedArrivalAt { get; set; }
    public DateTime? PlannedDepartureAt { get; set; }
    public int? ServiceDurationMinutes { get; set; }

    public string? Instructions { get; set; }
}

public class CreateDeliveryRouteRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? PlannedStartAt { get; set; }
    public DateTime? PlannedEndAt { get; set; }

    public string? StartLocationLabel { get; set; }
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }
    public string? EndLocationLabel { get; set; }
    public double? EndLatitude { get; set; }
    public double? EndLongitude { get; set; }

    public Guid? OriginDistrictId { get; set; }
    public decimal? VehicleCapacityKg { get; set; }
    public string? Notes { get; set; }

    public List<RouteStopInput> Stops { get; set; } = new();
}

public class UpdateDeliveryRouteRequest
{
    public string? Name { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? PlannedStartAt { get; set; }
    public DateTime? PlannedEndAt { get; set; }

    public string? StartLocationLabel { get; set; }
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }
    public string? EndLocationLabel { get; set; }
    public double? EndLatitude { get; set; }
    public double? EndLongitude { get; set; }

    public Guid? OriginDistrictId { get; set; }
    public decimal? VehicleCapacityKg { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public string? Notes { get; set; }
}

public class UpdateRouteStopRequest
{
    public string? StopType { get; set; }
    public Guid? PickupRequestId { get; set; }
    public Guid? OrderId { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public Guid? DistrictId { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public decimal? LoadKg { get; set; }
    public int? PackageCount { get; set; }
    public DateTime? PlannedArrivalAt { get; set; }
    public DateTime? PlannedDepartureAt { get; set; }
    public int? ServiceDurationMinutes { get; set; }
    public string? Instructions { get; set; }
}

public class ResequenceRouteRequest
{
    /// <summary>Stop ids in the desired order. Must contain every stop on the route exactly once.</summary>
    public List<Guid> StopIdsInOrder { get; set; } = new();
}

public class OptimizeRouteRequest
{
    /// <summary>Currently only <c>nearest-neighbor</c> is supported. Defaults to that.</summary>
    public string? Strategy { get; set; }

    /// <summary>Average travel speed used to estimate leg durations, km/h. Defaults to 25.</summary>
    public double? AverageSpeedKmh { get; set; }
}

public class AssignRouteRequest
{
    public string AssignedDriverName { get; set; } = string.Empty;
    public string? AssignedDriverPhone { get; set; }
    public string? AssignedVehicleLabel { get; set; }
    public decimal? VehicleCapacityKg { get; set; }
    public string? Note { get; set; }
}

public class RouteTransitionRequest
{
    public string? Note { get; set; }
}

public class CancelRouteRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CompleteRouteStopRequest
{
    public string? CompletionNote { get; set; }
    public DateTime? ActualArrivalAt { get; set; }
    public DateTime? ActualDepartureAt { get; set; }
}

public class FailRouteStopRequest
{
    public string FailureReason { get; set; } = string.Empty;
}

public class AddRouteNoteRequest
{
    public string Note { get; set; } = string.Empty;
}

public class DeliveryRouteQueryParameters
{
    public string? Status { get; set; }
    public DateTime? ScheduledFrom { get; set; }
    public DateTime? ScheduledTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
