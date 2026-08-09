using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICSRSponsorshipService
{
    Task<OpportunityDto> CreateOpportunityAsync(Guid producerId, CreateOpportunityRequest request, CancellationToken cancellationToken);
    Task<PagedResult<OpportunityDto>> GetOpportunitiesAsync(OpportunityQueryParameters parameters, CancellationToken cancellationToken);
    Task<PagedResult<OpportunityDto>> GetOpportunitiesForProducerAsync(Guid producerId, OpportunityQueryParameters parameters, CancellationToken cancellationToken);
    Task<OpportunityDto> GetOpportunityByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<OpportunityDto> CloseOpportunityAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken);
    Task<OpportunityDto> CancelOpportunityAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken);

    Task<ProposalDto> SubmitProposalAsync(Guid opportunityId, Guid businessPartnerId, SubmitProposalRequest request, CancellationToken cancellationToken);
    Task<PagedResult<ProposalListItemDto>> GetProposalsForBusinessPartnerAsync(Guid businessPartnerId, ProposalQueryParameters parameters, CancellationToken cancellationToken);
    Task<PagedResult<ProposalListItemDto>> GetProposalsForOpportunityAsync(Guid opportunityId, Guid producerId, bool isAdmin, ProposalQueryParameters parameters, CancellationToken cancellationToken);
    Task<ProposalDto> GetProposalByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<ProposalDto> DecideProposalAsync(Guid id, Guid producerId, bool isAdmin, ProposalDecisionRequest request, CancellationToken cancellationToken);
    Task<SponsorshipMilestoneDto> AddMilestoneAsync(Guid id, Guid currentUserId, bool isAdmin, SponsorshipMilestoneInput request, CancellationToken cancellationToken);
    Task<SponsorshipMilestoneDto> UpdateMilestoneStatusAsync(Guid id, Guid milestoneId, Guid currentUserId, bool isAdmin, UpdateMilestoneStatusRequest request, CancellationToken cancellationToken);
    Task<ProgressUpdateDto> AddProgressUpdateAsync(Guid id, Guid currentUserId, bool isAdmin, AddProgressUpdateRequest request, CancellationToken cancellationToken);
    Task<ImpactRecordDto> AddImpactRecordAsync(Guid id, Guid currentUserId, bool isAdmin, AddImpactRecordRequest request, CancellationToken cancellationToken);
    Task<ProposalDto> CompleteProposalAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<ProposalDto> CancelProposalAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}
