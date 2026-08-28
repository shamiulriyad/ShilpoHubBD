using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ManufacturingPartnership;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Application.Services.ManufacturingPartnership;

public class PartnershipService : IPartnershipService
{
    private readonly IPartnershipRepository _partnershipRepository;
    private readonly IUserRepository _userRepository;

    public PartnershipService(IPartnershipRepository partnershipRepository, IUserRepository userRepository)
    {
        _partnershipRepository = partnershipRepository;
        _userRepository = userRepository;
    }

    public async Task<PartnershipDto> CreateAsync(Guid businessPartnerId, CreatePartnershipRequest request, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdWithRolesAsync(request.ProducerId, cancellationToken);
        if (producer is null || !producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("Producer not found.");
        }

        var now = DateTime.UtcNow;
        var partnership = new Domain.Entities.ManufacturingPartnership.ManufacturingPartnership
        {
            Id = Guid.NewGuid(),
            BusinessPartnerId = businessPartnerId,
            ProducerId = request.ProducerId,
            ReferenceNumber = await GenerateUniqueReferenceNumberAsync(cancellationToken),
            Title = request.Title.Trim(),
            ProductRequirements = request.ProductRequirements.Trim(),
            ManufacturingSpecifications = request.ManufacturingSpecifications.Trim(),
            Quantity = request.Quantity,
            TargetUnitPrice = request.TargetUnitPrice,
            TimelineStartDate = request.TimelineStartDate,
            TimelineEndDate = request.TimelineEndDate,
            Status = PartnershipStatus.Requested,
            CreatedAt = now,
            UpdatedAt = now,
        };

        for (var i = 0; i < request.Milestones.Count; i++)
        {
            var milestone = request.Milestones[i];
            partnership.Milestones.Add(new ManufacturingMilestone
            {
                Id = Guid.NewGuid(),
                Title = milestone.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(milestone.Description) ? null : milestone.Description.Trim(),
                DueDate = milestone.DueDate,
                Status = MilestoneStatus.Pending,
                DisplayOrder = i,
            });
        }

        partnership.StatusHistory.Add(new PartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = PartnershipStatus.Requested,
            Note = "Partnership request sent to producer.",
            CreatedAt = now,
        });

        await _partnershipRepository.AddAsync(partnership, cancellationToken);
        await _partnershipRepository.SaveChangesAsync(cancellationToken);

        var created = await _partnershipRepository.GetByIdWithDetailsAsync(partnership.Id, cancellationToken)
            ?? throw new NotFoundException("Manufacturing partnership not found.");
        return ToDto(created);
    }

    public async Task<PagedResult<PartnershipListItemDto>> GetForBusinessPartnerAsync(
        Guid businessPartnerId, bool isAdmin, PartnershipQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = isAdmin
            ? await _partnershipRepository.GetPagedAllAsync(parameters, cancellationToken)
            : await _partnershipRepository.GetPagedForBusinessPartnerAsync(businessPartnerId, parameters, cancellationToken);

        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<PagedResult<PartnershipListItemDto>> GetForProducerAsync(
        Guid producerId, PartnershipQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _partnershipRepository.GetPagedForProducerAsync(producerId, parameters, cancellationToken);
        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<PartnershipDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var partnership = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);
        return ToDto(partnership);
    }

    public async Task<PartnershipDto> RespondAsync(Guid id, Guid producerId, PartnershipResponseRequest request, CancellationToken cancellationToken)
    {
        var partnership = await _partnershipRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Manufacturing partnership not found.");

        if (partnership.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to respond to this partnership request.");
        }

        if (partnership.Status != PartnershipStatus.Requested)
        {
            throw new ConflictException("Only a requested partnership can be responded to.");
        }

        var now = DateTime.UtcNow;
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        var newStatus = request.Accept ? PartnershipStatus.InProgress : PartnershipStatus.Rejected;

        partnership.Status = newStatus;
        partnership.ProducerResponseNotes = notes;
        partnership.RespondedAt = now;
        partnership.UpdatedAt = now;
        partnership.StatusHistory.Add(new PartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = newStatus,
            Note = notes,
            CreatedAt = now,
        });

        await _partnershipRepository.SaveChangesAsync(cancellationToken);
        return ToDto(partnership);
    }

    public async Task<MilestoneDto> AddMilestoneAsync(
        Guid id, Guid currentUserId, bool isAdmin, MilestoneInput request, CancellationToken cancellationToken)
    {
        var partnership = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        if (partnership.Status is not (PartnershipStatus.Requested or PartnershipStatus.InProgress))
        {
            throw new ConflictException("Milestones can only be added to a requested or in-progress partnership.");
        }

        var milestone = new ManufacturingMilestone
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DueDate = request.DueDate,
            Status = MilestoneStatus.Pending,
            DisplayOrder = partnership.Milestones.Count,
        };

        partnership.Milestones.Add(milestone);
        partnership.UpdatedAt = DateTime.UtcNow;

        await _partnershipRepository.SaveChangesAsync(cancellationToken);

        return ToMilestoneDto(milestone);
    }

    public async Task<MilestoneDto> UpdateMilestoneStatusAsync(
        Guid id, Guid milestoneId, Guid currentUserId, bool isAdmin, UpdateMilestoneStatusRequest request, CancellationToken cancellationToken)
    {
        var milestone = await _partnershipRepository.GetMilestoneAsync(id, milestoneId, cancellationToken)
            ?? throw new NotFoundException("Milestone not found.");

        if (!isAdmin && milestone.Partnership.BusinessPartnerId != currentUserId && milestone.Partnership.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this milestone.");
        }

        milestone.Status = request.Status;
        milestone.CompletedAt = request.Status == MilestoneStatus.Completed ? DateTime.UtcNow : null;
        milestone.Partnership.UpdatedAt = DateTime.UtcNow;

        await _partnershipRepository.SaveChangesAsync(cancellationToken);

        return ToMilestoneDto(milestone);
    }

    public async Task<PartnershipDto> CompleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var partnership = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        if (partnership.Status != PartnershipStatus.InProgress)
        {
            throw new ConflictException("Only an in-progress partnership can be marked as completed.");
        }

        var now = DateTime.UtcNow;
        partnership.Status = PartnershipStatus.Completed;
        partnership.CompletedAt = now;
        partnership.UpdatedAt = now;
        partnership.StatusHistory.Add(new PartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = PartnershipStatus.Completed,
            CreatedAt = now,
        });

        await _partnershipRepository.SaveChangesAsync(cancellationToken);
        return ToDto(partnership);
    }

    public async Task<PartnershipDto> CancelAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var partnership = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        if (partnership.Status is PartnershipStatus.Completed or PartnershipStatus.Cancelled or PartnershipStatus.Rejected)
        {
            throw new ConflictException("This partnership can no longer be cancelled.");
        }

        var now = DateTime.UtcNow;
        partnership.Status = PartnershipStatus.Cancelled;
        partnership.UpdatedAt = now;
        partnership.StatusHistory.Add(new PartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = PartnershipStatus.Cancelled,
            CreatedAt = now,
        });

        await _partnershipRepository.SaveChangesAsync(cancellationToken);
        return ToDto(partnership);
    }

    private async Task<Domain.Entities.ManufacturingPartnership.ManufacturingPartnership> GetPartyAsync(
        Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var partnership = await _partnershipRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Manufacturing partnership not found.");

        if (!isAdmin && partnership.BusinessPartnerId != currentUserId && partnership.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this partnership.");
        }

        return partnership;
    }

    private async Task<string> GenerateUniqueReferenceNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        string referenceNumber;

        do
        {
            referenceNumber = $"MFG-{year}-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _partnershipRepository.ExistsByReferenceNumberAsync(referenceNumber, cancellationToken));

        return referenceNumber;
    }

    private static int ComputeProgress(Domain.Entities.ManufacturingPartnership.ManufacturingPartnership partnership)
    {
        if (partnership.Status == PartnershipStatus.Completed)
        {
            return 100;
        }

        if (partnership.Milestones.Count == 0)
        {
            return 0;
        }

        var completed = partnership.Milestones.Count(m => m.Status == MilestoneStatus.Completed);
        return (int)Math.Round(completed * 100.0 / partnership.Milestones.Count);
    }

    private static PagedResult<PartnershipListItemDto> ToPagedListDto(
        List<Domain.Entities.ManufacturingPartnership.ManufacturingPartnership> items, int totalCount, PartnershipQueryParameters parameters)
    {
        return new PagedResult<PartnershipListItemDto>
        {
            Items = items.Select(p => new PartnershipListItemDto
            {
                Id = p.Id,
                ReferenceNumber = p.ReferenceNumber,
                Title = p.Title,
                ProducerName = p.Producer.FullName,
                Quantity = p.Quantity,
                TimelineEndDate = p.TimelineEndDate,
                Status = p.Status,
                ProgressPercentage = ComputeProgress(p),
                CreatedAt = p.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private static MilestoneDto ToMilestoneDto(ManufacturingMilestone milestone) => new()
    {
        Id = milestone.Id,
        Title = milestone.Title,
        Description = milestone.Description,
        DueDate = milestone.DueDate,
        Status = milestone.Status,
        CompletedAt = milestone.CompletedAt,
        DisplayOrder = milestone.DisplayOrder,
    };

    private static PartnershipDto ToDto(Domain.Entities.ManufacturingPartnership.ManufacturingPartnership partnership) => new()
    {
        Id = partnership.Id,
        ReferenceNumber = partnership.ReferenceNumber,
        BusinessPartnerId = partnership.BusinessPartnerId,
        BusinessPartnerName = partnership.BusinessPartner.FullName,
        ProducerId = partnership.ProducerId,
        ProducerName = partnership.Producer.FullName,
        Title = partnership.Title,
        ProductRequirements = partnership.ProductRequirements,
        ManufacturingSpecifications = partnership.ManufacturingSpecifications,
        Quantity = partnership.Quantity,
        TargetUnitPrice = partnership.TargetUnitPrice,
        TimelineStartDate = partnership.TimelineStartDate,
        TimelineEndDate = partnership.TimelineEndDate,
        Status = partnership.Status,
        ProducerResponseNotes = partnership.ProducerResponseNotes,
        RespondedAt = partnership.RespondedAt,
        CompletedAt = partnership.CompletedAt,
        ProgressPercentage = ComputeProgress(partnership),
        Milestones = partnership.Milestones
            .OrderBy(m => m.DisplayOrder)
            .Select(ToMilestoneDto)
            .ToList(),
        StatusHistory = partnership.StatusHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new PartnershipStatusEventDto
            {
                Status = h.Status,
                Note = h.Note,
                CreatedAt = h.CreatedAt,
            }).ToList(),
        CreatedAt = partnership.CreatedAt,
        UpdatedAt = partnership.UpdatedAt,
    };
}
