namespace ShilpoHubBD.Application.DTOs.Logistics;

public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;

    public Guid LogisticsPartnerProfileId { get; set; }
    public string? LogisticsPartnerName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? ContactPersonName { get; set; }
    public string? ContactPhone { get; set; }

    public int TotalCapacityUnits { get; set; }
    public int UsedCapacityUnits { get; set; }

    public bool HasColdChain { get; set; }
    public bool HandlesHazardous { get; set; }
    public bool HandlesReturns { get; set; }
    public string? Notes { get; set; }

    public int ZoneCount { get; set; }
    public int BinCount { get; set; }
    public int StockItemCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<WarehouseZoneDto> Zones { get; set; } = new();
    public List<WarehouseBinDto> Bins { get; set; } = new();
}

public class WarehouseListItemDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? DistrictName { get; set; }
    public int TotalCapacityUnits { get; set; }
    public int UsedCapacityUnits { get; set; }
    public bool HasColdChain { get; set; }
    public int ZoneCount { get; set; }
    public int BinCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WarehouseZoneDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsColdChain { get; set; }
    public int CapacityUnits { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class WarehouseBinDto
{
    public Guid Id { get; set; }
    public Guid? WarehouseZoneId { get; set; }
    public string? ZoneCode { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string Type { get; set; } = string.Empty;
    public int CapacityUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public bool IsPickable { get; set; }
    public bool IsActive { get; set; }
}
