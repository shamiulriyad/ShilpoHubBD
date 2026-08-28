using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

/// <summary>
/// Government &amp; NGO compliance tracking: a record per producer / village / district / product /
/// organisation against a framework, with a requirement checklist. The rolled-up score and status
/// are derived from mandatory-requirement completion unless a status is set explicitly.
/// </summary>
public class ComplianceService : IComplianceService
{
    private readonly IComplianceRepository _repository;

    public ComplianceService(IComplianceRepository repository)
    {
        _repository = repository;
    }

    public async Task<ComplianceRecordDto> CreateAsync(
        Guid userId, CreateComplianceRecordRequest request, CancellationToken cancellationToken)
    {
        var entityType = ParseEnum<ComplianceEntityType>(request.EntityType, "Invalid EntityType.");

        if (request.ReviewerUserId is { } r && !await _repository.UserExistsAsync(r, cancellationToken))
        {
            throw new NotFoundException("Reviewer user not found.");
        }

        var now = DateTime.UtcNow;
        var record = new ComplianceRecord
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = request.EntityId,
            EntityLabel = request.EntityLabel.Trim(),
            Framework = request.Framework.Trim(),
            PeriodStart = request.PeriodStart.HasValue
                ? DateTime.SpecifyKind(request.PeriodStart.Value, DateTimeKind.Utc)
                : now,
            PeriodEnd = ToUtc(request.PeriodEnd),
            NextReviewDue = ToUtc(request.NextReviewDue),
            ReviewerUserId = request.ReviewerUserId,
            Notes = request.Notes?.Trim(),
            CreatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var order = 0;
        foreach (var req in request.Requirements)
        {
            record.Requirements.Add(new ComplianceRequirement
            {
                Id = Guid.NewGuid(),
                ComplianceRecordId = record.Id,
                Code = req.Code.Trim(),
                Title = req.Title.Trim(),
                Description = req.Description?.Trim(),
                IsMandatory = req.IsMandatory,
                Status = ParseEnum<ComplianceRequirementStatus>(req.Status, "Invalid requirement Status."),
                Evidence = req.Evidence?.Trim(),
                ReviewedAt = null,
                DisplayOrder = req.DisplayOrder == 0 ? order : req.DisplayOrder,
            });
            order++;
        }

        Recalculate(record, explicitStatus: null, now);

        await _repository.AddAsync(record, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(record.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<ComplianceRecordListItemDto>> GetPagedAsync(
        ComplianceQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);

        return new PagedResult<ComplianceRecordListItemDto>
        {
            Items = items.Select(r => r.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ComplianceRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => (await LoadAsync(id, cancellationToken)).ToDto();

    public async Task<ComplianceRecordDto> UpdateAsync(
        Guid userId, Guid id, UpdateComplianceRecordRequest request, CancellationToken cancellationToken)
    {
        var record = await LoadAsync(id, cancellationToken);

        if (request.ReviewerUserId is { } r && !await _repository.UserExistsAsync(r, cancellationToken))
        {
            throw new NotFoundException("Reviewer user not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Framework))
        {
            record.Framework = request.Framework.Trim();
        }

        if (request.PeriodEnd.HasValue)
        {
            record.PeriodEnd = ToUtc(request.PeriodEnd);
        }

        if (request.NextReviewDue.HasValue)
        {
            record.NextReviewDue = ToUtc(request.NextReviewDue);
        }

        if (request.ReviewerUserId.HasValue)
        {
            record.ReviewerUserId = request.ReviewerUserId;
        }

        if (request.Notes is not null)
        {
            record.Notes = request.Notes.Trim();
        }

        var now = DateTime.UtcNow;
        if (request.MarkReviewedNow)
        {
            record.LastReviewedAt = now;
        }

        ComplianceStatus? explicitStatus = string.IsNullOrWhiteSpace(request.Status)
            ? null
            : ParseEnum<ComplianceStatus>(request.Status, "Invalid Status.");

        Recalculate(record, explicitStatus, now);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<ComplianceRecordDto> UpsertRequirementAsync(
        Guid userId, Guid id, UpsertComplianceRequirementRequest request, CancellationToken cancellationToken)
    {
        var record = await LoadAsync(id, cancellationToken);
        var status = ParseEnum<ComplianceRequirementStatus>(request.Status, "Invalid requirement Status.");
        var now = DateTime.UtcNow;

        ComplianceRequirement requirement;
        if (request.Id is { } reqId)
        {
            requirement = record.Requirements.FirstOrDefault(x => x.Id == reqId)
                ?? throw new NotFoundException("Requirement not found on this record.");
        }
        else
        {
            requirement = new ComplianceRequirement
            {
                Id = Guid.NewGuid(),
                ComplianceRecordId = record.Id,
            };
            record.Requirements.Add(requirement);
        }

        requirement.Code = request.Code.Trim();
        requirement.Title = request.Title.Trim();
        requirement.Description = request.Description?.Trim();
        requirement.IsMandatory = request.IsMandatory;
        requirement.Status = status;
        requirement.Evidence = request.Evidence?.Trim();
        requirement.ReviewedAt = now;
        requirement.DisplayOrder = request.DisplayOrder == 0 ? record.Requirements.Count : request.DisplayOrder;

        Recalculate(record, explicitStatus: null, now);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<ComplianceRecordDto> RemoveRequirementAsync(
        Guid userId, Guid id, Guid requirementId, CancellationToken cancellationToken)
    {
        var record = await LoadAsync(id, cancellationToken);
        var requirement = record.Requirements.FirstOrDefault(x => x.Id == requirementId)
            ?? throw new NotFoundException("Requirement not found on this record.");

        record.Requirements.Remove(requirement);
        Recalculate(record, explicitStatus: null, DateTime.UtcNow);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await LoadAsync(id, cancellationToken);
        _repository.Remove(record);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers -------------------------------------------------------

    private async Task<ComplianceRecord> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Compliance record not found.");

    private static void Recalculate(ComplianceRecord record, ComplianceStatus? explicitStatus, DateTime now)
    {
        var mandatory = record.Requirements
            .Where(r => r.IsMandatory && r.Status != ComplianceRequirementStatus.NotApplicable)
            .ToList();

        decimal score;
        if (mandatory.Count == 0)
        {
            score = record.Requirements.Count == 0 ? 0 : 100;
        }
        else
        {
            var points = mandatory.Sum(r => r.Status switch
            {
                ComplianceRequirementStatus.Met => 1.0m,
                ComplianceRequirementStatus.Partial => 0.5m,
                _ => 0m,
            });
            score = Math.Round(points / mandatory.Count * 100, 2);
        }

        record.OverallScorePercent = score;
        record.UpdatedAt = now;

        if (explicitStatus is { } forced)
        {
            record.Status = forced;
            return;
        }

        if (record.Status == ComplianceStatus.Waived)
        {
            return; // keep waived until explicitly changed
        }

        if (record.PeriodEnd.HasValue && record.PeriodEnd.Value < now)
        {
            record.Status = ComplianceStatus.Expired;
            return;
        }

        var anyReviewed = record.Requirements.Any(r => r.ReviewedAt != null)
            || record.Requirements.Any(r => r.Status != ComplianceRequirementStatus.Unmet);

        record.Status = record.Requirements.Count == 0 || !anyReviewed
            ? ComplianceStatus.NotStarted
            : score >= 100
                ? ComplianceStatus.Compliant
                : mandatory.Any(r => r.Status == ComplianceRequirementStatus.Unmet) && score < 60
                    ? ComplianceStatus.NonCompliant
                    : ComplianceStatus.InProgress;
    }

    private static DateTime? ToUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}
