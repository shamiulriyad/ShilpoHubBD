namespace ShilpoHubBD.Domain.Entities.Procurement;

/// <summary>Admin inspection of a bulk deal once the business partner has paid the advance.</summary>
public enum ProcurementInspectionStatus
{
    /// <summary>No advance paid yet, so nothing to inspect.</summary>
    NotRequired = 0,

    /// <summary>Advance paid; waiting for an admin to inspect the deal.</summary>
    Pending = 1,

    Approved = 2,
    Rejected = 3,
}
