using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Investment;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IInvestmentService
{
    Task<InvestmentOpportunityDto> CreateOpportunityAsync(Guid producerId, CreateInvestmentOpportunityRequest request, CancellationToken cancellationToken);
    Task<PagedResult<InvestmentOpportunityDto>> GetOpportunitiesAsync(InvestmentOpportunityQueryParameters parameters, CancellationToken cancellationToken);
    Task<PagedResult<InvestmentOpportunityDto>> GetOpportunitiesForProducerAsync(Guid producerId, InvestmentOpportunityQueryParameters parameters, CancellationToken cancellationToken);
    Task<InvestmentOpportunityDto> GetOpportunityByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<InvestmentOpportunityDto> CloseOpportunityAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken);
    Task<InvestmentOpportunityDto> CancelOpportunityAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken);

    Task<InvestmentProposalDto> SubmitProposalAsync(Guid opportunityId, Guid businessPartnerId, SubmitInvestmentProposalRequest request, CancellationToken cancellationToken);
    Task<PagedResult<InvestmentProposalListItemDto>> GetProposalsForBusinessPartnerAsync(Guid businessPartnerId, InvestmentProposalQueryParameters parameters, CancellationToken cancellationToken);
    Task<PagedResult<InvestmentProposalListItemDto>> GetProposalsForOpportunityAsync(Guid opportunityId, Guid producerId, bool isAdmin, InvestmentProposalQueryParameters parameters, CancellationToken cancellationToken);
    Task<InvestmentProposalDto> GetProposalByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<InvestmentProposalDto> DecideProposalAsync(Guid id, Guid producerId, bool isAdmin, InvestmentProposalDecisionRequest request, CancellationToken cancellationToken);
    Task<InvestmentMilestoneDto> AddMilestoneAsync(Guid id, Guid currentUserId, bool isAdmin, InvestmentMilestoneInput request, CancellationToken cancellationToken);
    Task<InvestmentMilestoneDto> UpdateMilestoneStatusAsync(Guid id, Guid milestoneId, Guid currentUserId, bool isAdmin, UpdateInvestmentMilestoneStatusRequest request, CancellationToken cancellationToken);
    Task<InvestmentDocumentDto> AddDocumentAsync(Guid id, Guid currentUserId, bool isAdmin, AddInvestmentDocumentRequest request, CancellationToken cancellationToken);
    Task<InvestmentProposalDto> CompleteProposalAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<InvestmentProposalDto> CancelProposalAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}
