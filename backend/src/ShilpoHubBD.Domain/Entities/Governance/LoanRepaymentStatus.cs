namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>Repayment position of a funded <see cref="FundingApplication"/> under a loan programme.</summary>
public enum LoanRepaymentStatus
{
    NotApplicable,
    Pending,
    InRepayment,
    Repaid,
    Defaulted,
}
