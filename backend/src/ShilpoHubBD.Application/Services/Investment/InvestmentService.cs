using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Investment;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Application.Services.Investment;

public class InvestmentService : IInvestmentService
{
    private readonly IInvestmentRepository _repository;
    private readonly IUserRepository _userRepository;

    public InvestmentService(IInvestmentRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<InvestmentOpportunityDto> CreateOpportunityAsync(
        Guid producerId, CreateInvestmentOpportunityRequest request, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var now = DateTime.UtcNow;
        var opportunity = new InvestmentOpportunity
        {
            Id = Guid.NewGuid(),
            ProducerId = producerId,
            Title = request.Title.Trim(),
            ProjectDescription = request.ProjectDescription.Trim(),
            FundingRequirement = request.FundingRequirement,
            Status = InvestmentOpportunityStatus.Open,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddOpportunityAsync(opportunity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        opportunity.Producer = producer;
        return ToOpportunityDto(opportunity);
    }

    public async Task<PagedResult<InvestmentOpportunityDto>> GetOpportunitiesAsync(
        InvestmentOpportunityQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedOpportunitiesAsync(parameters, cancellationToken);
        return ToPagedOpportunityDto(items, totalCount, parameters);
    }

    public async Task<PagedResult<InvestmentOpportunityDto>> GetOpportunitiesForProducerAsync(
        Guid producerId, InvestmentOpportunityQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedOpportunitiesForProducerAsync(producerId, parameters, cancellationToken);
        return ToPagedOpportunityDto(items, totalCount, parameters);
    }

    public async Task<InvestmentOpportunityDto> GetOpportunityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var opportunity = await _repository.GetOpportunityByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Investment opportunity not found.");
        return ToOpportunityDto(opportunity);
    }

    public async Task<InvestmentOpportunityDto> CloseOpportunityAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var opportunity = await GetOwnedOpportunityAsync(id, producerId, isAdmin, cancellationToken);

        if (opportunity.Status != InvestmentOpportunityStatus.Open)
        {
            throw new ConflictException("Only an open opportunity can be closed.");
        }

        opportunity.Status = InvestmentOpportunityStatus.Closed;
        opportunity.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToOpportunityDto(opportunity);
    }

    public async Task<InvestmentOpportunityDto> CancelOpportunityAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var opportunity = await GetOwnedOpportunityAsync(id, producerId, isAdmin, cancellationToken);

        if (opportunity.Status is InvestmentOpportunityStatus.Cancelled or InvestmentOpportunityStatus.FullyFunded)
        {
            throw new ConflictException("This opportunity can no longer be cancelled.");
        }

        opportunity.Status = InvestmentOpportunityStatus.Cancelled;
        opportunity.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToOpportunityDto(opportunity);
    }

    public async Task<InvestmentProposalDto> SubmitProposalAsync(
        Guid opportunityId, Guid businessPartnerId, SubmitInvestmentProposalRequest request, CancellationToken cancellationToken)
    {
        var opportunity = await _repository.GetOpportunityByIdAsync(opportunityId, cancellationToken)
            ?? throw new NotFoundException("Investment opportunity not found.");

        if (opportunity.Status != InvestmentOpportunityStatus.Open)
        {
            throw new ConflictException("This opportunity is no longer open for proposals.");
        }

        var now = DateTime.UtcNow;
        var proposal = new InvestmentProposal
        {
            Id = Guid.NewGuid(),
            OpportunityId = opportunityId,
            BusinessPartnerId = businessPartnerId,
            InvestmentAmount = request.InvestmentAmount,
            ProposalMessage = string.IsNullOrWhiteSpace(request.ProposalMessage) ? null : request.ProposalMessage.Trim(),
            Status = InvestmentProposalStatus.Submitted,
            SubmittedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        proposal.StatusHistory.Add(new InvestmentStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = InvestmentProposalStatus.Submitted,
            CreatedAt = now,
        });

        await _repository.AddProposalAsync(proposal, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var created = await _repository.GetProposalByIdAsync(proposal.Id, cancellationToken)
            ?? throw new NotFoundException("Investment proposal not found.");
        return ToProposalDto(created);
    }

    public async Task<PagedResult<InvestmentProposalListItemDto>> GetProposalsForBusinessPartnerAsync(
        Guid businessPartnerId, InvestmentProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedProposalsForBusinessPartnerAsync(businessPartnerId, parameters, cancellationToken);
        return ToPagedProposalListDto(items, totalCount, parameters);
    }

    public async Task<PagedResult<InvestmentProposalListItemDto>> GetProposalsForOpportunityAsync(
        Guid opportunityId, Guid producerId, bool isAdmin, InvestmentProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        await GetOwnedOpportunityAsync(opportunityId, producerId, isAdmin, cancellationToken);

        var (items, totalCount) = await _repository.GetPagedProposalsForOpportunityAsync(opportunityId, parameters, cancellationToken);
        return ToPagedProposalListDto(items, totalCount, parameters);
    }

    public async Task<InvestmentProposalDto> GetProposalByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);
        return ToProposalDto(proposal);
    }

    public async Task<InvestmentProposalDto> DecideProposalAsync(
        Guid id, Guid producerId, bool isAdmin, InvestmentProposalDecisionRequest request, CancellationToken cancellationToken)
    {
        var proposal = await _repository.GetProposalByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Investment proposal not found.");

        if (!isAdmin && proposal.Opportunity.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to decide on this proposal.");
        }

        if (proposal.Status != InvestmentProposalStatus.Submitted)
        {
            throw new ConflictException("This proposal has already been decided.");
        }

        var now = DateTime.UtcNow;
        var newStatus = request.Approve ? InvestmentProposalStatus.Active : InvestmentProposalStatus.Rejected;

        proposal.Status = newStatus;
        proposal.DecidedAt = now;
        proposal.DecisionNotes = string.IsNullOrWhiteSpace(request.DecisionNotes) ? null : request.DecisionNotes.Trim();
        proposal.UpdatedAt = now;
        proposal.StatusHistory.Add(new InvestmentStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = newStatus,
            Note = proposal.DecisionNotes,
            CreatedAt = now,
        });

        if (request.Approve)
        {
            var fundingSecured = proposal.Opportunity.Proposals
                .Where(p => p.Status is InvestmentProposalStatus.Active or InvestmentProposalStatus.Completed)
                .Sum(p => p.InvestmentAmount) + proposal.InvestmentAmount;

            if (fundingSecured >= proposal.Opportunity.FundingRequirement)
            {
                proposal.Opportunity.Status = InvestmentOpportunityStatus.FullyFunded;
                proposal.Opportunity.UpdatedAt = now;
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return ToProposalDto(proposal);
    }

    public async Task<InvestmentMilestoneDto> AddMilestoneAsync(
        Guid id, Guid currentUserId, bool isAdmin, InvestmentMilestoneInput request, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        var milestone = new InvestmentMilestone
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DueDate = request.DueDate,
            Status = InvestmentMilestoneStatus.Pending,
            DisplayOrder = proposal.Milestones.Count,
        };

        proposal.Milestones.Add(milestone);
        proposal.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToMilestoneDto(milestone);
    }

    public async Task<InvestmentMilestoneDto> UpdateMilestoneStatusAsync(
        Guid id, Guid milestoneId, Guid currentUserId, bool isAdmin, UpdateInvestmentMilestoneStatusRequest request, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        var milestone = proposal.Milestones.FirstOrDefault(m => m.Id == milestoneId)
            ?? throw new NotFoundException("Milestone not found.");

        milestone.Status = request.Status;
        milestone.CompletedAt = request.Status == InvestmentMilestoneStatus.Completed ? DateTime.UtcNow : null;
        proposal.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToMilestoneDto(milestone);
    }

    public async Task<InvestmentDocumentDto> AddDocumentAsync(
        Guid id, Guid currentUserId, bool isAdmin, AddInvestmentDocumentRequest request, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        var document = new InvestmentDocument
        {
            Id = Guid.NewGuid(),
            DocumentName = request.DocumentName.Trim(),
            DocumentType = request.DocumentType.Trim(),
            FileUrl = request.FileUrl.Trim(),
            UploadedAt = DateTime.UtcNow,
        };

        proposal.Documents.Add(document);
        proposal.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        return new InvestmentDocumentDto
        {
            Id = document.Id,
            DocumentName = document.DocumentName,
            DocumentType = document.DocumentType,
            FileUrl = document.FileUrl,
            UploadedAt = document.UploadedAt,
        };
    }

    public async Task<InvestmentProposalDto> CompleteProposalAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        if (proposal.Status != InvestmentProposalStatus.Active)
        {
            throw new ConflictException("Only an active investment can be marked as completed.");
        }

        var now = DateTime.UtcNow;
        proposal.Status = InvestmentProposalStatus.Completed;
        proposal.CompletedAt = now;
        proposal.UpdatedAt = now;
        proposal.StatusHistory.Add(new InvestmentStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = InvestmentProposalStatus.Completed,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return ToProposalDto(proposal);
    }

    public async Task<InvestmentProposalDto> CancelProposalAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        if (proposal.Status is InvestmentProposalStatus.Completed or InvestmentProposalStatus.Cancelled or InvestmentProposalStatus.Rejected)
        {
            throw new ConflictException("This proposal can no longer be cancelled.");
        }

        var now = DateTime.UtcNow;
        proposal.Status = InvestmentProposalStatus.Cancelled;
        proposal.UpdatedAt = now;
        proposal.StatusHistory.Add(new InvestmentStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = InvestmentProposalStatus.Cancelled,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return ToProposalDto(proposal);
    }

    private async Task<InvestmentOpportunity> GetOwnedOpportunityAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var opportunity = await _repository.GetOpportunityByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Investment opportunity not found.");

        if (!isAdmin && opportunity.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this opportunity.");
        }

        return opportunity;
    }

    private async Task<InvestmentProposal> GetPartyProposalAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var proposal = await _repository.GetProposalByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Investment proposal not found.");

        if (!isAdmin && proposal.BusinessPartnerId != currentUserId && proposal.Opportunity.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this proposal.");
        }

        return proposal;
    }

    private static PagedResult<InvestmentOpportunityDto> ToPagedOpportunityDto(
        List<InvestmentOpportunity> items, int totalCount, InvestmentOpportunityQueryParameters parameters)
    {
        return new PagedResult<InvestmentOpportunityDto>
        {
            Items = items.Select(ToOpportunityDto).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private static InvestmentOpportunityDto ToOpportunityDto(InvestmentOpportunity opportunity) => new()
    {
        Id = opportunity.Id,
        ProducerId = opportunity.ProducerId,
        ProducerName = opportunity.Producer.FullName,
        Title = opportunity.Title,
        ProjectDescription = opportunity.ProjectDescription,
        FundingRequirement = opportunity.FundingRequirement,
        FundingSecured = opportunity.Proposals
            .Where(p => p.Status is InvestmentProposalStatus.Active or InvestmentProposalStatus.Completed)
            .Sum(p => p.InvestmentAmount),
        Status = opportunity.Status,
        ProposalCount = opportunity.Proposals.Count,
        CreatedAt = opportunity.CreatedAt,
        UpdatedAt = opportunity.UpdatedAt,
    };

    private static PagedResult<InvestmentProposalListItemDto> ToPagedProposalListDto(
        List<InvestmentProposal> items, int totalCount, InvestmentProposalQueryParameters parameters)
    {
        return new PagedResult<InvestmentProposalListItemDto>
        {
            Items = items.Select(p => new InvestmentProposalListItemDto
            {
                Id = p.Id,
                OpportunityId = p.OpportunityId,
                OpportunityTitle = p.Opportunity.Title,
                BusinessPartnerName = p.BusinessPartner?.FullName ?? string.Empty,
                InvestmentAmount = p.InvestmentAmount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private static InvestmentMilestoneDto ToMilestoneDto(InvestmentMilestone milestone) => new()
    {
        Id = milestone.Id,
        Title = milestone.Title,
        Description = milestone.Description,
        DueDate = milestone.DueDate,
        Status = milestone.Status,
        CompletedAt = milestone.CompletedAt,
        DisplayOrder = milestone.DisplayOrder,
    };

    private static InvestmentProposalDto ToProposalDto(InvestmentProposal proposal) => new()
    {
        Id = proposal.Id,
        OpportunityId = proposal.OpportunityId,
        OpportunityTitle = proposal.Opportunity.Title,
        BusinessPartnerId = proposal.BusinessPartnerId,
        BusinessPartnerName = proposal.BusinessPartner.FullName,
        InvestmentAmount = proposal.InvestmentAmount,
        ProposalMessage = proposal.ProposalMessage,
        Status = proposal.Status,
        SubmittedAt = proposal.SubmittedAt,
        DecidedAt = proposal.DecidedAt,
        DecisionNotes = proposal.DecisionNotes,
        CompletedAt = proposal.CompletedAt,
        Milestones = proposal.Milestones.OrderBy(m => m.DisplayOrder).Select(ToMilestoneDto).ToList(),
        Documents = proposal.Documents
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new InvestmentDocumentDto
            {
                Id = d.Id,
                DocumentName = d.DocumentName,
                DocumentType = d.DocumentType,
                FileUrl = d.FileUrl,
                UploadedAt = d.UploadedAt,
            }).ToList(),
        StatusHistory = proposal.StatusHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new InvestmentStatusEventDto
            {
                Status = h.Status,
                Note = h.Note,
                CreatedAt = h.CreatedAt,
            }).ToList(),
        CreatedAt = proposal.CreatedAt,
        UpdatedAt = proposal.UpdatedAt,
    };
}
