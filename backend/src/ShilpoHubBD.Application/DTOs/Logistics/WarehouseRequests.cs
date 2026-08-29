namespace ShilpoHubBD.Application.DTOs.Logistics;

public class CreateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Distribution, Fulfillment, ColdStorage, CrossDock, Returns or Hub.</summary>
    public string? Type { get; set; }

    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Guid? DistrictId { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? ContactPersonName { get; set; }
    public string? ContactPhone { get; set; }

    public int TotalCapacityUnits { get; set; }
    public bool HasColdChain { get; set; }
    public bool HandlesHazardous { get; set; }
    public bool HandlesReturns { get; set; }
    public string? Notes { get; set; }
}

public class UpdateWarehouseRequest
{
    public string? Name { get; set; }
    public string? Type { get; set; }

    /// <summary>Active, Inactive, Maintenance or Closed.</summary>
    public string? Status { get; set; }

    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public Guid? DistrictId { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ContactPersonName { get; set; }
    public string? ContactPhone { get; set; }
    public int? TotalCapacityUnits { get; set; }
    public bool? HasColdChain { get; set; }
    public bool? HandlesHazardous { get; set; }
    public bool? HandlesReturns { get; set; }
    public string? Notes { get; set; }
}

public class UpsertWarehouseZoneRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Receiving, Storage, Picking, Packing, Dispatch, Returns, ColdStorage, Quarantine or Staging.</summary>
    public string Type { get; set; } = string.Empty;

    public bool IsColdChain { get; set; }
    public int CapacityUnits { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpsertWarehouseBinRequest
{
    public Guid? WarehouseZoneId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }

    /// <summary>Shelf, Rack, Pallet, Floor, Bulk, Bin or ColdUnit.</summary>
    public string Type { get; set; } = string.Empty;

    public int CapacityUnits { get; set; }
    public bool IsPickable { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class WarehouseQueryParameters
{
    public string? Type { get; set; }
    public string? Status { get; set; }
    public Guid? DistrictId { get; set; }
    public bool? HasColdChain { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
