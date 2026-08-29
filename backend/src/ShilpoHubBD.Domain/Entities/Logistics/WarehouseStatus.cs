namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Operational state of a <see cref="Warehouse"/>.</summary>
public enum WarehouseStatus
{
    Active,
    Inactive,
    Maintenance,
    Closed,
}
