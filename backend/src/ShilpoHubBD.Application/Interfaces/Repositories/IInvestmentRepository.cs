using ShilpoHubBD.Application.DTOs.Investment;
using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IInvestmentRepository
{
    Task<InvestmentOpportunity?> GetOpportunityByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<InvestmentOpportunity> Items, int TotalCount)> GetPagedOpportunitiesAsync(InvestmentOpportunityQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<InvestmentOpportunity> Items, int TotalCount)> GetPagedOpportunitiesForProducerAsync(Guid producerId, InvestmentOpportunityQueryParameters parameters, CancellationToken cancellationToken);
    Task AddOpportunityAsync(InvestmentOpportunity opportunity, CancellationToken cancellationToken);

    Task<InvestmentProposal?> GetProposalByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<InvestmentProposal> Items, int TotalCount)> GetPagedProposalsForBusinessPartnerAsync(Guid businessPartnerId, InvestmentProposalQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<InvestmentProposal> Items, int TotalCount)> GetPagedProposalsForOpportunityAsync(Guid opportunityId, InvestmentProposalQueryParameters parameters, CancellationToken cancellationToken);
    Task AddProposalAsync(InvestmentProposal proposal, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
