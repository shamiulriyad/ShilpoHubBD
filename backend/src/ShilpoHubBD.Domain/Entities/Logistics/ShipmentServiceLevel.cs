namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Delivery speed tier promised for a <see cref="Shipment"/>.</summary>
public enum ShipmentServiceLevel
{
    Economy,
    Standard,
    Express,
    SameDay,
}
