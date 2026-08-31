namespace ShilpoHubBD.Domain.Entities.Governance;

public enum FundingApplicationEventType
{
    Submitted,
    StatusChanged,
    Reviewed,
    Approved,
    Rejected,
    Withdrawn,
    DisbursementScheduled,
    DisbursementPaid,
    RepaymentRecorded,
    Note,
}
