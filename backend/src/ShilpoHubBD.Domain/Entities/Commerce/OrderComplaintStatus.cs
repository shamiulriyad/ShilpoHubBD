namespace ShilpoHubBD.Domain.Entities.Commerce;

public enum OrderComplaintStatus
{
    /// <summary>Filed by the customer; the producer has not fixed it yet.</summary>
    Open = 0,

    /// <summary>The producer says it is sorted out; the customer decides whether they agree.</summary>
    Resolved = 1,

    /// <summary>The customer confirmed they are satisfied. Only now can they rate the producer and product.</summary>
    Satisfied = 2,

    Withdrawn = 3,
}
