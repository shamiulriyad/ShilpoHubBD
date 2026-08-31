namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Why a <see cref="ReturnRequest"/> was raised.</summary>
public enum ReturnReason
{
    DamagedInTransit,
    DefectiveProduct,
    WrongItem,
    NotAsDescribed,
    CustomerChangedMind,
    DeliveryFailed,
    Undeliverable,
    LateDelivery,
    Other,
}
