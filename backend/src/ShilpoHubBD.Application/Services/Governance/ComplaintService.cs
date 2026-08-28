using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

/// <summary>
/// Government &amp; NGO complaint desk: intake, triage, an update thread (with internal notes),
/// assignment, resolution, and linking to a monitoring flag.
/// </summary>
public class ComplaintService : IComplaintService
{
    private readonly IComplaintRepository _repository;

    public ComplaintService(IComplaintRepository repository)
    {
        _repository = repository;
    }

    public async Task<ComplaintDto> CreateAsync(
        Guid userId, CreateComplaintRequest request, CancellationToken cancellationToken)
    {
        var category = ParseEnum<ComplaintCategory>(request.Category, "Invalid Category.");
        var priority = ParseEnum<ComplaintPriority>(request.Priority, "Invalid Priority.");
        var againstType = ParseEnum<MonitoringSubjectType>(request.AgainstType, "Invalid AgainstType.");

        if (request.ComplainantUserId is { } cu && !await _repository.UserExistsAsync(cu, cancellationToken))
        {
            throw new NotFoundException("Complainant user not found.");
        }

        if (request.RelatedOrderId is { } oid && !await _repository.OrderExistsAsync(oid, cancellationToken))
        {
            throw new NotFoundException("Related order not found.");
        }

        var now = DateTime.UtcNow;
        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            ReferenceCode = await GenerateReferenceAsync(now, cancellationToken),
            Category = category,
            Priority = priority,
            Status = ComplaintStatus.Submitted,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ComplainantUserId = request.ComplainantUserId,
            ComplainantName = request.ComplainantName?.Trim(),
            ComplainantContact = request.ComplainantContact?.Trim(),
            AgainstType = againstType,
            AgainstId = request.AgainstId,
            AgainstLabel = request.AgainstLabel?.Trim(),
            RelatedOrderId = request.RelatedOrderId,
            CreatedAt = now,
            UpdatedAt = now,
        };
        complaint.Updates.Add(new ComplaintUpdate
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaint.Id,
            Message = "Complaint received.",
            IsInternal = false,
            ToStatus = ComplaintStatus.Submitted,
            ActorUserId = userId,
            CreatedAt = now,
        });

        await _repository.AddAsync(complaint, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(complaint.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<ComplaintListItemDto>> GetPagedAsync(
        ComplaintQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);

        return new PagedResult<ComplaintListItemDto>
        {
            Items = items.Select(c => c.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ComplaintDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => (await LoadAsync(id, cancellationToken)).ToDto();

    public async Task<ComplaintDto> UpdateAsync(
        Guid userId, Guid id, UpdateComplaintRequest request, CancellationToken cancellationToken)
    {
        var complaint = await LoadAsync(id, cancellationToken);
        EnsureMutable(complaint);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            complaint.Category = ParseEnum<ComplaintCategory>(request.Category, "Invalid Category.");
        }

        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            complaint.Priority = ParseEnum<ComplaintPriority>(request.Priority, "Invalid Priority.");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            complaint.Title = request.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            complaint.Description = request.Description.Trim();
        }

        if (request.AgainstLabel is not null)
        {
            complaint.AgainstLabel = request.AgainstLabel.Trim();
        }

        complaint.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<ComplaintDto> AddUpdateAsync(
        Guid userId, Guid id, AddComplaintUpdateRequest request, CancellationToken cancellationToken)
    {
        var complaint = await LoadAsync(id, cancellationToken);
        var now = DateTime.UtcNow;

        ComplaintStatus? from = null;
        ComplaintStatus? to = null;
        if (!string.IsNullOrWhiteSpace(request.NewStatus))
        {
            var newStatus = ParseEnum<ComplaintStatus>(request.NewStatus, "Invalid NewStatus.");
            if (newStatus != complaint.Status)
            {
                if (complaint.Status is ComplaintStatus.Closed)
                {
                    throw new ConflictException("A closed complaint cannot change status.");
                }

                from = complaint.Status;
                to = newStatus;
                complaint.Status = newStatus;
                if (newStatus is ComplaintStatus.Resolved or ComplaintStatus.Rejected && complaint.ResolvedAt is null)
                {
                    complaint.ResolvedAt = now;
                    complaint.ResolvedByUserId = userId;
                }
            }
        }

        complaint.Updates.Add(new ComplaintUpdate
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaint.Id,
            Message = request.Message.Trim(),
            IsInternal = request.IsInternal,
            FromStatus = from,
            ToStatus = to,
            ActorUserId = userId,
            CreatedAt = now,
        });
        complaint.UpdatedAt = now;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<ComplaintDto> AssignAsync(
        Guid userId, Guid id, AssignComplaintRequest request, CancellationToken cancellationToken)
    {
        var complaint = await LoadAsync(id, cancellationToken);
        EnsureMutable(complaint);

        if (!await _repository.UserExistsAsync(request.AssigneeUserId, cancellationToken))
        {
            throw new NotFoundException("Assignee user not found.");
        }

        var now = DateTime.UtcNow;
        complaint.AssignedToUserId = request.AssigneeUserId;
        var from = complaint.Status;
        if (complaint.Status == ComplaintStatus.Submitted)
        {
            complaint.Status = ComplaintStatus.Triaged;
        }

        complaint.UpdatedAt = now;
        complaint.Updates.Add(new ComplaintUpdate
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaint.Id,
            Message = string.IsNullOrWhiteSpace(request.Note) ? "Complaint assigned." : request.Note.Trim(),
            IsInternal = true,
            FromStatus = from == complaint.Status ? null : from,
            ToStatus = from == complaint.Status ? null : complaint.Status,
            ActorUserId = userId,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<ComplaintDto> ResolveAsync(
        Guid userId, Guid id, ResolveComplaintRequest request, CancellationToken cancellationToken)
    {
        var complaint = await LoadAsync(id, cancellationToken);
        if (complaint.Status is ComplaintStatus.Closed)
        {
            throw new ConflictException("Complaint is already closed.");
        }

        var outcome = ParseEnum<ComplaintStatus>(request.Outcome, "Outcome must be Resolved or Rejected.");
        if (outcome is not (ComplaintStatus.Resolved or ComplaintStatus.Rejected))
        {
            throw new ConflictException("Outcome must be Resolved or Rejected.");
        }

        var now = DateTime.UtcNow;
        var from = complaint.Status;
        complaint.Status = outcome;
        complaint.Resolution = request.Resolution.Trim();
        complaint.ResolvedAt = now;
        complaint.ResolvedByUserId = userId;
        complaint.UpdatedAt = now;
        complaint.Updates.Add(new ComplaintUpdate
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaint.Id,
            Message = request.Resolution.Trim(),
            IsInternal = false,
            FromStatus = from,
            ToStatus = outcome,
            ActorUserId = userId,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<ComplaintDto> LinkFlagAsync(
        Guid userId, Guid id, LinkComplaintFlagRequest request, CancellationToken cancellationToken)
    {
        var complaint = await LoadAsync(id, cancellationToken);

        if (!await _repository.FlagExistsAsync(request.MonitoringFlagId, cancellationToken))
        {
            throw new NotFoundException("Monitoring flag not found.");
        }

        var now = DateTime.UtcNow;
        complaint.MonitoringFlagId = request.MonitoringFlagId;
        complaint.UpdatedAt = now;
        complaint.Updates.Add(new ComplaintUpdate
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaint.Id,
            Message = "Linked to a monitoring flag.",
            IsInternal = true,
            ActorUserId = userId,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var complaint = await LoadAsync(id, cancellationToken);
        _repository.Remove(complaint);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers -------------------------------------------------------

    private async Task<Complaint> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Complaint not found.");

    private static void EnsureMutable(Complaint complaint)
    {
        if (complaint.Status is ComplaintStatus.Closed or ComplaintStatus.Resolved or ComplaintStatus.Rejected)
        {
            throw new ConflictException($"A {complaint.Status} complaint can no longer be edited.");
        }
    }

    private async Task<string> GenerateReferenceAsync(DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 6; attempt++)
        {
            var candidate = $"CMP-{now:yyyyMM}-{Random.Shared.Next(0, 100000):D5}";
            if (!await _repository.ReferenceExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return $"CMP-{now:yyyyMM}-{Guid.NewGuid():N}"[..20];
    }

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}
