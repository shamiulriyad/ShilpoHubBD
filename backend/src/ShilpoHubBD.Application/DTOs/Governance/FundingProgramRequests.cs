namespace ShilpoHubBD.Application.DTOs.Governance;

public class CreateFundingProgramRequest
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Grant, Loan, Scholarship, EquipmentSupport, VillageSponsorship or ProducerSponsorship.</summary>
    public string Type { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string? EligibilityCriteria { get; set; }
    public string Currency { get; set; } = "BDT";

    public decimal TotalBudget { get; set; }
    public decimal? MinAmountPerApplicant { get; set; }
    public decimal? MaxAmountPerApplicant { get; set; }
    public DateTime? ApplicationOpensAt { get; set; }
    public DateTime? ApplicationClosesAt { get; set; }

    // Loan terms — ignored unless Type == Loan.
    public bool RequiresRepayment { get; set; }
    public decimal? InterestRatePercent { get; set; }
    public int? RepaymentPeriodMonths { get; set; }
}

public class UpdateFundingProgramRequest
{
    public string? Name { get; set; }

    /// <summary>Draft, Open, Closed or Archived.</summary>
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string? EligibilityCriteria { get; set; }
    public decimal? TotalBudget { get; set; }
    public decimal? MinAmountPerApplicant { get; set; }
    public decimal? MaxAmountPerApplicant { get; set; }
    public DateTime? ApplicationOpensAt { get; set; }
    public DateTime? ApplicationClosesAt { get; set; }
    public bool? RequiresRepayment { get; set; }
    public decimal? InterestRatePercent { get; set; }
    public int? RepaymentPeriodMonths { get; set; }
}

public class FundingProgramQueryParameters
{
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
