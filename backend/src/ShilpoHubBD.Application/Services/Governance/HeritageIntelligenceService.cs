using System.Text.Json;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

/// <summary>
/// Government &amp; NGO Heritage Intelligence: computes six explainable composite indices
/// (risk, living-heritage, craft-health, village-survival, youth-participation, climate-risk) from
/// live platform signals via a replaceable rule-based provider, and stores the results for trend
/// tracking. Youth / training-pipeline signals are platform-wide aggregates regardless of scope,
/// since apprenticeship and academy records carry no district link.
/// </summary>
public class HeritageIntelligenceService : IHeritageIntelligenceService
{
    private const int MaxTrendPoints = 60;
    private const int DefaultWindowMonths = 12;

    private readonly IHeritageIntelligenceRepository _repository;
    private readonly IHeritageIntelligenceProvider _provider;

    public HeritageIntelligenceService(
        IHeritageIntelligenceRepository repository, IHeritageIntelligenceProvider provider)
    {
        _repository = repository;
        _provider = provider;
    }

    public async Task<HeritageIndexRecordDto> ComputeAsync(
        Guid userId, ComputeHeritageIndexRequest request, CancellationToken cancellationToken)
    {
        var indexType = ParseEnum<HeritageIndexType>(request.IndexType,
            "IndexType must be one of: HeritageRiskIndex, LivingHeritageIndex, CraftHealthScore, "
            + "VillageSurvivalIndex, YouthParticipation, ClimateRiskAnalysis.");
        var scope = ParseEnum<HeritageIndexScope>(request.Scope,
            "Scope must be one of: National, District, Village, Craft.");

        var (to, from) = ResolveWindow(request.PeriodStart, request.PeriodEnd);

        Guid? scopeId = null;
        string scopeLabel;
        string? craftLabel = null;

        switch (scope)
        {
            case HeritageIndexScope.National:
                scopeLabel = "National";
                break;

            case HeritageIndexScope.District:
                if (request.ScopeId is not { } districtId)
                {
                    throw new ConflictException("ScopeId (district id) is required for District scope.");
                }

                var district = await _repository.GetDistrictAsync(districtId, cancellationToken)
                    ?? throw new NotFoundException("District not found.");
                scopeId = district.Id;
                scopeLabel = district.Name;
                break;

            case HeritageIndexScope.Village:
                if (request.ScopeId is not { } villageId)
                {
                    throw new ConflictException("ScopeId (village id) is required for Village scope.");
                }

                var village = await _repository.GetVillageAsync(villageId, cancellationToken)
                    ?? throw new NotFoundException("Village not found.");
                scopeId = village.Id;
                scopeLabel = village.Name;
                craftLabel = string.IsNullOrWhiteSpace(request.CraftLabel) ? village.Craft : request.CraftLabel.Trim();
                break;

            case HeritageIndexScope.Craft:
                if (string.IsNullOrWhiteSpace(request.CraftLabel))
                {
                    throw new ConflictException("CraftLabel is required for Craft scope.");
                }

                craftLabel = request.CraftLabel.Trim();
                scopeLabel = craftLabel;
                break;

            default:
                throw new ConflictException("Unsupported scope.");
        }

        var signals = await _repository.GatherSignalsAsync(
            scope, scopeId, craftLabel, from, to, cancellationToken);

        var computation = _provider.Compute(indexType, scope, scopeLabel, signals);

        var now = DateTime.UtcNow;
        var record = new HeritageIndexRecord
        {
            Id = Guid.NewGuid(),
            IndexType = indexType,
            Scope = scope,
            ScopeId = scopeId,
            ScopeLabel = scopeLabel,
            Score = computation.Score,
            Rating = computation.Rating,
            Method = computation.Method,
            Summary = computation.Summary,
            PeriodStart = from,
            PeriodEnd = to,
            ComputedAt = now,
            SignalsJson = JsonSerializer.Serialize(signals),
            Notes = request.Notes?.Trim(),
            GeneratedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var order = 0;
        foreach (var c in computation.Components)
        {
            record.Components.Add(new HeritageIndexComponent
            {
                Id = Guid.NewGuid(),
                HeritageIndexRecordId = record.Id,
                Key = c.Key,
                Label = c.Label,
                RawValue = c.RawValue,
                Weight = c.Weight,
                ContributionScore = c.ContributionScore,
                Detail = c.Detail,
                DisplayOrder = order++,
            });
        }

        if (!request.Persist)
        {
            return record.ToDto();
        }

        await _repository.AddAsync(record, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(record.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<HeritageIndexRecordListItemDto>> GetRecordsAsync(
        HeritageIndexQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);

        return new PagedResult<HeritageIndexRecordListItemDto>
        {
            Items = items.Select(r => r.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<HeritageIndexRecordDto> GetRecordByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage index record not found.");
        return record.ToDto();
    }

    public async Task DeleteRecordAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage index record not found.");

        _repository.Remove(record);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<HeritageIndexTrendDto> GetTrendAsync(
        string indexType, string scope, Guid? scopeId, string? craftLabel, int take,
        CancellationToken cancellationToken)
    {
        var parsedType = ParseEnum<HeritageIndexType>(indexType, "Invalid IndexType.");
        var parsedScope = ParseEnum<HeritageIndexScope>(scope, "Invalid Scope.");

        if (parsedScope is HeritageIndexScope.District or HeritageIndexScope.Village && !scopeId.HasValue)
        {
            throw new ConflictException("scopeId is required for District / Village scope.");
        }

        if (parsedScope == HeritageIndexScope.Craft && string.IsNullOrWhiteSpace(craftLabel))
        {
            throw new ConflictException("craftLabel is required for Craft scope.");
        }

        take = Math.Clamp(take, 2, MaxTrendPoints);

        var records = await _repository.GetForTrendAsync(
            parsedType, parsedScope, scopeId, craftLabel, take, cancellationToken);
        records.Reverse();

        var points = new List<HeritageIndexTrendPointDto>();
        decimal? previous = null;
        foreach (var r in records)
        {
            points.Add(new HeritageIndexTrendPointDto
            {
                RecordId = r.Id,
                PeriodEnd = r.PeriodEnd,
                ComputedAt = r.ComputedAt,
                Score = r.Score,
                Rating = r.Rating.ToString(),
                ChangePoints = previous.HasValue ? Math.Round(r.Score - previous.Value, 2) : null,
            });
            previous = r.Score;
        }

        return new HeritageIndexTrendDto
        {
            IndexType = parsedType.ToString(),
            Scope = parsedScope.ToString(),
            ScopeLabel = records.FirstOrDefault()?.ScopeLabel
                ?? craftLabel?.Trim()
                ?? (parsedScope == HeritageIndexScope.National ? "National" : string.Empty),
            Points = points,
        };
    }

    // ---- helpers -----------------------------------------------------

    private static (DateTime To, DateTime From) ResolveWindow(DateTime? start, DateTime? end)
    {
        var to = end.HasValue ? DateTime.SpecifyKind(end.Value, DateTimeKind.Utc) : DateTime.UtcNow;
        var from = start.HasValue
            ? DateTime.SpecifyKind(start.Value, DateTimeKind.Utc)
            : to.AddMonths(-DefaultWindowMonths);

        if (to <= from)
        {
            throw new ConflictException("PeriodEnd must be after PeriodStart.");
        }

        return (to, from);
    }

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}
