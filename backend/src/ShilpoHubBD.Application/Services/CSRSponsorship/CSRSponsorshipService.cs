using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Application.Services.CSRSponsorship;

public class CSRSponsorshipService : ICSRSponsorshipService
{
    private readonly ICSRSponsorshipRepository _repository;
    private readonly IUserRepository _userRepository;

    public CSRSponsorshipService(ICSRSponsorshipRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<OpportunityDto> CreateOpportunityAsync(Guid producerId, CreateOpportunityRequest request, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var now = DateTime.UtcNow;
        var opportunity = new SponsorshipOpportunity
        {
            Id = Guid.NewGuid(),
            ProducerId = producerId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            FundingGoal = request.FundingGoal,
            Status = SponsorshipOpportunityStatus.Open,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddOpportunityAsync(opportunity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        opportunity.Producer = producer;
        return ToOpportunityDto(opportunity);
    }

    public async Task<PagedResult<OpportunityDto>> GetOpportunitiesAsync(OpportunityQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedOpportunitiesAsync(parameters, cancellationToken);
        return ToPagedOpportunityDto(items, totalCount, parameters);
    }

    public async Task<PagedResult<OpportunityDto>> GetOpportunitiesForProducerAsync(
        Guid producerId, OpportunityQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedOpportunitiesForProducerAsync(producerId, parameters, cancellationToken);
        return ToPagedOpportunityDto(items, totalCount, parameters);
    }

    public async Task<OpportunityDto> GetOpportunityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var opportunity = await _repository.GetOpportunityByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Sponsorship opportunity not found.");
        return ToOpportunityDto(opportunity);
    }

    public async Task<OpportunityDto> CloseOpportunityAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var opportunity = await GetOwnedOpportunityAsync(id, producerId, isAdmin, cancellationToken);

        if (opportunity.Status != SponsorshipOpportunityStatus.Open)
        {
            throw new ConflictException("Only an open opportunity can be closed.");
        }

        opportunity.Status = SponsorshipOpportunityStatus.Closed;
        opportunity.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToOpportunityDto(opportunity);
    }

    public async Task<OpportunityDto> CancelOpportunityAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var opportunity = await GetOwnedOpportunityAsync(id, producerId, isAdmin, cancellationToken);

        if (opportunity.Status is SponsorshipOpportunityStatus.Cancelled or SponsorshipOpportunityStatus.FullyFunded)
        {
            throw new ConflictException("This opportunity can no longer be cancelled.");
        }

        opportunity.Status = SponsorshipOpportunityStatus.Cancelled;
        opportunity.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToOpportunityDto(opportunity);
    }

    public async Task<ProposalDto> SubmitProposalAsync(
        Guid opportunityId, Guid businessPartnerId, SubmitProposalRequest request, CancellationToken cancellationToken)
    {
        var opportunity = await _repository.GetOpportunityByIdAsync(opportunityId, cancellationToken)
            ?? throw new NotFoundException("Sponsorship opportunity not found.");

        if (opportunity.Status != SponsorshipOpportunityStatus.Open)
        {
            throw new ConflictException("This opportunity is no longer open for proposals.");
        }

        var now = DateTime.UtcNow;
        var proposal = new SponsorshipProposal
        {
            Id = Guid.NewGuid(),
            OpportunityId = opportunityId,
            BusinessPartnerId = businessPartnerId,
            FundingAmount = request.FundingAmount,
            ProposalMessage = string.IsNullOrWhiteSpace(request.ProposalMessage) ? null : request.ProposalMessage.Trim(),
            Status = SponsorshipProposalStatus.Submitted,
            SubmittedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        proposal.StatusHistory.Add(new SponsorshipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = SponsorshipProposalStatus.Submitted,
            CreatedAt = now,
        });

        await _repository.AddProposalAsync(proposal, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var created = await _repository.GetProposalByIdAsync(proposal.Id, cancellationToken)
            ?? throw new NotFoundException("Sponsorship proposal not found.");
        return ToProposalDto(created);
    }

    public async Task<PagedResult<ProposalListItemDto>> GetProposalsForBusinessPartnerAsync(
        Guid businessPartnerId, ProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedProposalsForBusinessPartnerAsync(businessPartnerId, parameters, cancellationToken);
        return ToPagedProposalListDto(items, totalCount, parameters);
    }

    public async Task<PagedResult<ProposalListItemDto>> GetProposalsForOpportunityAsync(
        Guid opportunityId, Guid producerId, bool isAdmin, ProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        await GetOwnedOpportunityAsync(opportunityId, producerId, isAdmin, cancellationToken);

        var (items, totalCount) = await _repository.GetPagedProposalsForOpportunityAsync(opportunityId, parameters, cancellationToken);
        return ToPagedProposalListDto(items, totalCount, parameters);
    }

    public async Task<ProposalDto> GetProposalByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);
        return ToProposalDto(proposal);
    }

    public async Task<ProposalDto> DecideProposalAsync(
        Guid id, Guid producerId, bool isAdmin, ProposalDecisionRequest request, CancellationToken cancellationToken)
    {
        var proposal = await _repository.GetProposalByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Sponsorship proposal not found.");

        if (!isAdmin && proposal.Opportunity.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to decide on this proposal.");
        }

        if (proposal.Status != SponsorshipProposalStatus.Submitted)
        {
            throw new ConflictException("This proposal has already been decided.");
        }

        var now = DateTime.UtcNow;
        var newStatus = request.Approve ? SponsorshipProposalStatus.Active : SponsorshipProposalStatus.Rejected;

        proposal.Status = newStatus;
        proposal.DecidedAt = now;
        proposal.DecisionNotes = string.IsNullOrWhiteSpace(request.DecisionNotes) ? null : request.DecisionNotes.Trim();
        proposal.UpdatedAt = now;
        proposal.StatusHistory.Add(new SponsorshipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = newStatus,
            Note = proposal.DecisionNotes,
            CreatedAt = now,
        });

        if (request.Approve)
        {
            var fundingSecured = proposal.Opportunity.Proposals
                .Where(p => p.Status is SponsorshipProposalStatus.Active or SponsorshipProposalStatus.Completed)
                .Sum(p => p.FundingAmount) + proposal.FundingAmount;

            if (fundingSecured >= proposal.Opportunity.FundingGoal)
            {
                proposal.Opportunity.Status = SponsorshipOpportunityStatus.FullyFunded;
                proposal.Opportunity.UpdatedAt = now;
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return ToProposalDto(proposal);
    }

    public async Task<SponsorshipMilestoneDto> AddMilestoneAsync(
        Guid id, Guid currentUserId, bool isAdmin, SponsorshipMilestoneInput request, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        var milestone = new SponsorshipMilestone
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DueDate = request.DueDate,
            Status = SponsorshipMilestoneStatus.Pending,
            DisplayOrder = proposal.Milestones.Count,
        };

        proposal.Milestones.Add(milestone);
        proposal.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToMilestoneDto(milestone);
    }

    public async Task<SponsorshipMilestoneDto> UpdateMilestoneStatusAsync(
        Guid id, Guid milestoneId, Guid currentUserId, bool isAdmin, UpdateMilestoneStatusRequest request, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        var milestone = proposal.Milestones.FirstOrDefault(m => m.Id == milestoneId)
            ?? throw new NotFoundException("Milestone not found.");

        milestone.Status = request.Status;
        milestone.CompletedAt = request.Status == SponsorshipMilestoneStatus.Completed ? DateTime.UtcNow : null;
        proposal.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToMilestoneDto(milestone);
    }

    public async Task<ProgressUpdateDto> AddProgressUpdateAsync(
        Guid id, Guid currentUserId, bool isAdmin, AddProgressUpdateRequest request, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        var author = await _userRepository.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var update = new SponsorshipProgressUpdate
        {
            Id = Guid.NewGuid(),
            AuthorUserId = currentUserId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        proposal.ProgressUpdates.Add(update);
        proposal.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        return new ProgressUpdateDto
        {
            Id = update.Id,
            AuthorUserId = currentUserId,
            AuthorName = author.FullName,
            Content = update.Content,
            CreatedAt = update.CreatedAt,
        };
    }

    public async Task<ImpactRecordDto> AddImpactRecordAsync(
        Guid id, Guid currentUserId, bool isAdmin, AddImpactRecordRequest request, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        var record = new SponsorshipImpactRecord
        {
            Id = Guid.NewGuid(),
            Description = request.Description.Trim(),
            Metric = request.Metric.Trim(),
            Value = request.Value,
            RecordedAt = DateTime.UtcNow,
        };

        proposal.ImpactRecords.Add(record);
        proposal.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        return new ImpactRecordDto
        {
            Id = record.Id,
            Description = record.Description,
            Metric = record.Metric,
            Value = record.Value,
            RecordedAt = record.RecordedAt,
        };
    }

    public async Task<ProposalDto> CompleteProposalAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        if (proposal.Status != SponsorshipProposalStatus.Active)
        {
            throw new ConflictException("Only an active sponsorship can be marked as completed.");
        }

        var now = DateTime.UtcNow;
        proposal.Status = SponsorshipProposalStatus.Completed;
        proposal.CompletedAt = now;
        proposal.UpdatedAt = now;
        proposal.StatusHistory.Add(new SponsorshipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = SponsorshipProposalStatus.Completed,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return ToProposalDto(proposal);
    }

    public async Task<ProposalDto> CancelProposalAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var proposal = await GetPartyProposalAsync(id, currentUserId, isAdmin, cancellationToken);

        if (proposal.Status is SponsorshipProposalStatus.Completed or SponsorshipProposalStatus.Cancelled or SponsorshipProposalStatus.Rejected)
        {
            throw new ConflictException("This proposal can no longer be cancelled.");
        }

        var now = DateTime.UtcNow;
        proposal.Status = SponsorshipProposalStatus.Cancelled;
        proposal.UpdatedAt = now;
        proposal.StatusHistory.Add(new SponsorshipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = SponsorshipProposalStatus.Cancelled,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return ToProposalDto(proposal);
    }

    private async Task<SponsorshipOpportunity> GetOwnedOpportunityAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var opportunity = await _repository.GetOpportunityByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Sponsorship opportunity not found.");

        if (!isAdmin && opportunity.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this opportunity.");
        }

        return opportunity;
    }

    private async Task<SponsorshipProposal> GetPartyProposalAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var proposal = await _repository.GetProposalByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Sponsorship proposal not found.");

        if (!isAdmin && proposal.BusinessPartnerId != currentUserId && proposal.Opportunity.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this proposal.");
        }

        return proposal;
    }

    private static PagedResult<OpportunityDto> ToPagedOpportunityDto(
        List<SponsorshipOpportunity> items, int totalCount, OpportunityQueryParameters parameters)
    {
        return new PagedResult<OpportunityDto>
        {
            Items = items.Select(ToOpportunityDto).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private static OpportunityDto ToOpportunityDto(SponsorshipOpportunity opportunity) => new()
    {
        Id = opportunity.Id,
        ProducerId = opportunity.ProducerId,
        ProducerName = opportunity.Producer.FullName,
        Title = opportunity.Title,
        Description = opportunity.Description,
        FundingGoal = opportunity.FundingGoal,
        FundingSecured = opportunity.Proposals
            .Where(p => p.Status is SponsorshipProposalStatus.Active or SponsorshipProposalStatus.Completed)
            .Sum(p => p.FundingAmount),
        Status = opportunity.Status,
        ProposalCount = opportunity.Proposals.Count,
        CreatedAt = opportunity.CreatedAt,
        UpdatedAt = opportunity.UpdatedAt,
    };

    private static PagedResult<ProposalListItemDto> ToPagedProposalListDto(
        List<SponsorshipProposal> items, int totalCount, ProposalQueryParameters parameters)
    {
        return new PagedResult<ProposalListItemDto>
        {
            Items = items.Select(p => new ProposalListItemDto
            {
                Id = p.Id,
                OpportunityId = p.OpportunityId,
                OpportunityTitle = p.Opportunity.Title,
                BusinessPartnerName = p.BusinessPartner?.FullName ?? string.Empty,
                FundingAmount = p.FundingAmount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private static SponsorshipMilestoneDto ToMilestoneDto(SponsorshipMilestone milestone) => new()
    {
        Id = milestone.Id,
        Title = milestone.Title,
        Description = milestone.Description,
        DueDate = milestone.DueDate,
        Status = milestone.Status,
        CompletedAt = milestone.CompletedAt,
        DisplayOrder = milestone.DisplayOrder,
    };

    private static ProposalDto ToProposalDto(SponsorshipProposal proposal) => new()
    {
        Id = proposal.Id,
        OpportunityId = proposal.OpportunityId,
        OpportunityTitle = proposal.Opportunity.Title,
        BusinessPartnerId = proposal.BusinessPartnerId,
        BusinessPartnerName = proposal.BusinessPartner.FullName,
        FundingAmount = proposal.FundingAmount,
        ProposalMessage = proposal.ProposalMessage,
        Status = proposal.Status,
        SubmittedAt = proposal.SubmittedAt,
        DecidedAt = proposal.DecidedAt,
        DecisionNotes = proposal.DecisionNotes,
        CompletedAt = proposal.CompletedAt,
        Milestones = proposal.Milestones.OrderBy(m => m.DisplayOrder).Select(ToMilestoneDto).ToList(),
        ProgressUpdates = proposal.ProgressUpdates
            .OrderBy(u => u.CreatedAt)
            .Select(u => new ProgressUpdateDto
            {
                Id = u.Id,
                AuthorUserId = u.AuthorUserId,
                AuthorName = u.Author.FullName,
                Content = u.Content,
                CreatedAt = u.CreatedAt,
            }).ToList(),
        ImpactRecords = proposal.ImpactRecords
            .OrderByDescending(r => r.RecordedAt)
            .Select(r => new ImpactRecordDto
            {
                Id = r.Id,
                Description = r.Description,
                Metric = r.Metric,
                Value = r.Value,
                RecordedAt = r.RecordedAt,
            }).ToList(),
        StatusHistory = proposal.StatusHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new SponsorshipStatusEventDto
            {
                Status = h.Status,
                Note = h.Note,
                CreatedAt = h.CreatedAt,
            }).ToList(),
        CreatedAt = proposal.CreatedAt,
        UpdatedAt = proposal.UpdatedAt,
    };
}
