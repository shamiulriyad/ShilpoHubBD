using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Application.DTOs.Investment;

public class UpdateInvestmentMilestoneStatusRequest
{
    public InvestmentMilestoneStatus Status { get; set; }
}
