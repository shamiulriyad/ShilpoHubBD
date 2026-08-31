using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Services.HeritageDatabase;

public class HeritageRiskService : IHeritageRiskService
{
    private readonly IHeritageRiskRepository _repository;

    public HeritageRiskService(IHeritageRiskRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<HeritageRiskRecordDto>> GetPagedAsync(
        HeritageRiskQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<HeritageRiskRecordDto>
        {
            Items = items.Select(r => r.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<HeritageRiskRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage risk record not found.");
        return record.ToDto();
    }

    public async Task<HeritageRiskRecordDto> CreateAsync(
        Guid userId, CreateHeritageRiskRecordRequest request, CancellationToken cancellationToken)
    {
        var category = ParseCategory(request.Category);
        var level = ParseLevel(request.Level);
        await ValidateLinksAsync(request.DistrictId, request.VillageId, request.ProducerId, cancellationToken);

        var now = DateTime.UtcNow;
        var record = new HeritageRiskRecord
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = category,
            Level = level,
            CraftName = request.CraftName?.Trim(),
            DistrictId = request.DistrictId,
            VillageId = request.VillageId,
            ProducerId = request.ProducerId,
            AffectedArtisanCount = request.AffectedArtisanCount,
            ContributingFactors = request.ContributingFactors?.Trim(),
            RecommendedActions = request.RecommendedActions?.Trim(),
            Source = request.Source?.Trim(),
            AssessmentYear = request.AssessmentYear,
            AssessedOn = request.AssessedOn,
            CreatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(record, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(record.Id, cancellationToken))!.ToDto();
    }

    public async Task<HeritageRiskRecordDto> UpdateAsync(
        Guid userId, Guid id, UpdateHeritageRiskRecordRequest request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage risk record not found.");

        var category = ParseCategory(request.Category);
        var level = ParseLevel(request.Level);
        await ValidateLinksAsync(request.DistrictId, request.VillageId, request.ProducerId, cancellationToken);

        record.Title = request.Title.Trim();
        record.Description = request.Description.Trim();
        record.Category = category;
        record.Level = level;
        record.CraftName = request.CraftName?.Trim();
        record.DistrictId = request.DistrictId;
        record.VillageId = request.VillageId;
        record.ProducerId = request.ProducerId;
        record.AffectedArtisanCount = request.AffectedArtisanCount;
        record.ContributingFactors = request.ContributingFactors?.Trim();
        record.RecommendedActions = request.RecommendedActions?.Trim();
        record.Source = request.Source?.Trim();
        record.AssessmentYear = request.AssessmentYear;
        record.AssessedOn = request.AssessedOn;
        record.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(record.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage risk record not found.");

        _repository.Remove(record);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateLinksAsync(
        Guid? districtId, Guid? villageId, Guid? producerId, CancellationToken cancellationToken)
    {
        if (districtId.HasValue && !await _repository.DistrictExistsAsync(districtId.Value, cancellationToken))
        {
            throw new NotFoundException("District not found.");
        }

        if (villageId.HasValue && !await _repository.VillageExistsAsync(villageId.Value, cancellationToken))
        {
            throw new NotFoundException("Village not found.");
        }

        if (producerId.HasValue && !await _repository.ProducerExistsAsync(producerId.Value, cancellationToken))
        {
            throw new NotFoundException("Producer not found.");
        }
    }

    private static HeritageRiskCategory ParseCategory(string value)
        => Enum.TryParse<HeritageRiskCategory>(value, true, out var parsed)
            ? parsed
            : throw new ConflictException("Category must be a valid heritage risk category.");

    private static HeritageRiskLevel ParseLevel(string value)
        => Enum.TryParse<HeritageRiskLevel>(value, true, out var parsed)
            ? parsed
            : throw new ConflictException("Level must be one of: Low, Moderate, High, Critical, Safeguarded.");
}
