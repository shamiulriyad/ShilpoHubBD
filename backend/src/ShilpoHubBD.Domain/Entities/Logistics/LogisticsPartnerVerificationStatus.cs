namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Approval state of a <see cref="LogisticsPartnerProfile"/>.</summary>
public enum LogisticsPartnerVerificationStatus
{
    Pending,
    Verified,
    Rejected,
    Suspended,
}
