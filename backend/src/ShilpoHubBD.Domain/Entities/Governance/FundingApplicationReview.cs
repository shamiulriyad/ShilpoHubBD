using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>An assessor's review of a <see cref="FundingApplication"/>.</summary>
public class FundingApplicationReview
{
    public Guid Id { get; set; }

    public Guid FundingApplicationId { get; set; }
    public FundingApplication Application { get; set; } = null!;

    public Guid ReviewerUserId { get; set; }
    public User Reviewer { get; set; } = null!;

    public FundingReviewDecision Decision { get; set; }

    /// <summary>Optional 0–100 assessment score.</summary>
    public int? Score { get; set; }

    public decimal? RecommendedAmount { get; set; }
    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; }
}
