using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Services.HeritageDiscovery;

/// <summary>Admin-curated catalogue of UNESCO-recognised heritage sites, traditions and archives.</summary>
public class UnescoRecordService : IUnescoRecordService
{
    private readonly IUnescoRecordRepository _repository;

    public UnescoRecordService(IUnescoRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UnescoRecordDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
        => (await _repository.GetAllAsync(includeInactive, cancellationToken)).Select(ToDto).ToList();

    public async Task<UnescoRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => ToDto(await LoadAsync(id, cancellationToken));

    public async Task<UnescoRecordDto> CreateAsync(CreateUnescoRecordRequest request, CancellationToken cancellationToken)
    {
        var type = ParseEnum<UnescoRecordType>(request.Type, "Invalid Type.");
        await EnsureDistrictExistsAsync(request.DistrictId, cancellationToken);

        var now = DateTime.UtcNow;
        var record = new UnescoRecord
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Type = type,
            Description = request.Description.Trim(),
            InscribedYear = request.InscribedYear,
            DistrictId = request.DistrictId,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            OfficialUrl = string.IsNullOrWhiteSpace(request.OfficialUrl) ? null : request.OfficialUrl.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(record, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto((await _repository.GetByIdAsync(record.Id, cancellationToken))!);
    }

    public async Task<UnescoRecordDto> UpdateAsync(Guid id, UpdateUnescoRecordRequest request, CancellationToken cancellationToken)
    {
        var record = await LoadAsync(id, cancellationToken);
        var type = ParseEnum<UnescoRecordType>(request.Type, "Invalid Type.");
        await EnsureDistrictExistsAsync(request.DistrictId, cancellationToken);

        record.Title = request.Title.Trim();
        record.Type = type;
        record.Description = request.Description.Trim();
        record.InscribedYear = request.InscribedYear;
        record.DistrictId = request.DistrictId;
        record.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        record.OfficialUrl = string.IsNullOrWhiteSpace(request.OfficialUrl) ? null : request.OfficialUrl.Trim();
        record.DisplayOrder = request.DisplayOrder;
        record.IsActive = request.IsActive;
        record.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto((await _repository.GetByIdAsync(id, cancellationToken))!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await LoadAsync(id, cancellationToken);
        _repository.Remove(record);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<UnescoRecord> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("UNESCO record not found.");

    private async Task EnsureDistrictExistsAsync(Guid? districtId, CancellationToken cancellationToken)
    {
        if (districtId is { } id && !await _repository.DistrictExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException("District not found.");
        }
    }

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);

    private static UnescoRecordDto ToDto(UnescoRecord r) => new()
    {
        Id = r.Id,
        Title = r.Title,
        Type = r.Type.ToString(),
        Description = r.Description,
        InscribedYear = r.InscribedYear,
        DistrictId = r.DistrictId,
        DistrictName = r.District?.Name,
        ImageUrl = r.ImageUrl,
        OfficialUrl = r.OfficialUrl,
        DisplayOrder = r.DisplayOrder,
        IsActive = r.IsActive,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
    };
}
