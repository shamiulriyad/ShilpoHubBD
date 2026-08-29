namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Condition a returned item is in when assessed.</summary>
public enum ReturnItemCondition
{
    NotReceived,
    New,
    LikeNew,
    Used,
    Damaged,
    Defective,
    Unsalvageable,
}
