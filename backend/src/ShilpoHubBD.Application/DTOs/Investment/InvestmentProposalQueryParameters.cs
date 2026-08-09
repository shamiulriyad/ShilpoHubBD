using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Application.DTOs.Investment;

public class InvestmentProposalQueryParameters
{
    public InvestmentProposalStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
