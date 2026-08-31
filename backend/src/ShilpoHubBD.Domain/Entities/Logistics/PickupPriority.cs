namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>How urgently a <see cref="PickupRequest"/> must be serviced.</summary>
public enum PickupPriority
{
    Standard,
    Express,
    SameDay,
}
