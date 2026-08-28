using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// A funding window run by a Government / NGO body — a grant, loan, scholarship, equipment-support
/// scheme or a village / producer sponsorship. Holds the budget envelope and eligibility rules;
/// individual awards are tracked as <see cref="FundingApplication"/>s.
/// </summary>
public class FundingProgram
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public FundingProgramType Type { get; set; }
    public FundingProgramStatus Status { get; set; } = FundingProgramStatus.Draft;

    public string Description { get; set; } = string.Empty;
    public string? EligibilityCriteria { get; set; }
    public string Currency { get; set; } = "BDT";

    public decimal TotalBudget { get; set; }

    /// <summary>Sum of approved amounts across applications. Maintained on write.</summary>
    public decimal AllocatedAmount { get; set; }

    /// <summary>Sum of paid disbursements across applications. Maintained on write.</summary>
    public decimal DisbursedAmount { get; set; }

    public decimal? MinAmountPerApplicant { get; set; }
    public decimal? MaxAmountPerApplicant { get; set; }

    public DateTime? ApplicationOpensAt { get; set; }
    public DateTime? ApplicationClosesAt { get; set; }

    // ---- Loan terms (Type == Loan) ------------------------------------
    public bool RequiresRepayment { get; set; }
    public decimal? InterestRatePercent { get; set; }
    public int? RepaymentPeriodMonths { get; set; }

    public Guid ManagedByUserId { get; set; }
    public User ManagedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<FundingApplication> Applications { get; set; } = new List<FundingApplication>();
}
