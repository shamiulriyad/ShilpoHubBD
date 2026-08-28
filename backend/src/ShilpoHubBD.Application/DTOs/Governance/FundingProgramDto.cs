namespace ShilpoHubBD.Application.DTOs.Governance;

public class FundingProgramDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EligibilityCriteria { get; set; }
    public string Currency { get; set; } = "BDT";

    public decimal TotalBudget { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal DisbursedAmount { get; set; }
    public decimal RemainingBudget { get; set; }

    public decimal? MinAmountPerApplicant { get; set; }
    public decimal? MaxAmountPerApplicant { get; set; }
    public DateTime? ApplicationOpensAt { get; set; }
    public DateTime? ApplicationClosesAt { get; set; }

    public bool RequiresRepayment { get; set; }
    public decimal? InterestRatePercent { get; set; }
    public int? RepaymentPeriodMonths { get; set; }

    public Guid ManagedByUserId { get; set; }
    public string? ManagedByName { get; set; }

    public int ApplicationCount { get; set; }
    public int ApprovedCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FundingProgramListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = "BDT";
    public decimal TotalBudget { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal DisbursedAmount { get; set; }
    public DateTime? ApplicationClosesAt { get; set; }
    public int ApplicationCount { get; set; }
}
