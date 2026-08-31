namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Lifecycle of a <see cref="DeliveryRoute"/>.</summary>
public enum DeliveryRouteStatus
{
    /// <summary>Being assembled; stops can be added / reordered freely.</summary>
    Draft,

    /// <summary>Stops locked in and a crew assigned, ready to dispatch.</summary>
    Planned,

    /// <summary>Handed to the crew for execution.</summary>
    Dispatched,

    /// <summary>Crew has started running the route.</summary>
    InProgress,

    Completed,

    Cancelled,
}
