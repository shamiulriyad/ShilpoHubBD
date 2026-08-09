using ShilpoHubBD.Application.DTOs.CSRSponsorship;
using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICSRSponsorshipRepository
{
    Task<SponsorshipOpportunity?> GetOpportunityByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<SponsorshipOpportunity> Items, int TotalCount)> GetPagedOpportunitiesAsync(OpportunityQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<SponsorshipOpportunity> Items, int TotalCount)> GetPagedOpportunitiesForProducerAsync(Guid producerId, OpportunityQueryParameters parameters, CancellationToken cancellationToken);
    Task AddOpportunityAsync(SponsorshipOpportunity opportunity, CancellationToken cancellationToken);

    Task<SponsorshipProposal?> GetProposalByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<SponsorshipProposal> Items, int TotalCount)> GetPagedProposalsForBusinessPartnerAsync(Guid businessPartnerId, ProposalQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<SponsorshipProposal> Items, int TotalCount)> GetPagedProposalsForOpportunityAsync(Guid opportunityId, ProposalQueryParameters parameters, CancellationToken cancellationToken);
    Task AddProposalAsync(SponsorshipProposal proposal, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
