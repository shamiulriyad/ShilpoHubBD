using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Services.HeritageDiscovery;

public class HeritageFestivalService : IHeritageFestivalService
{
    private readonly IHeritageFestivalRepository _festivalRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly IHeritagePlaceRepository _placeRepository;

    public HeritageFestivalService(
        IHeritageFestivalRepository festivalRepository, IDistrictRepository districtRepository, IHeritagePlaceRepository placeRepository)
    {
        _festivalRepository = festivalRepository;
        _districtRepository = districtRepository;
        _placeRepository = placeRepository;
    }

    public async Task<PagedResult<HeritageFestivalDto>> GetPagedAsync(HeritageFestivalQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _festivalRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<HeritageFestivalDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<HeritageFestivalDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var festival = await _festivalRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage festival not found.");
        return ToDto(festival);
    }

    public async Task<HeritageFestivalDto> CreateAsync(CreateHeritageFestivalRequest request, CancellationToken cancellationToken)
    {
        await ValidateReferencesAsync(request.DistrictId, request.HeritagePlaceId, cancellationToken);

        if (request.EndDate < request.StartDate)
        {
            throw new ConflictException("End date cannot be earlier than start date.");
        }

        var now = DateTime.UtcNow;
        var festival = new HeritageFestival
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            DistrictId = request.DistrictId,
            HeritagePlaceId = request.HeritagePlaceId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsRecurringAnnually = request.IsRecurringAnnually,
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _festivalRepository.AddAsync(festival, cancellationToken);
        await _festivalRepository.SaveChangesAsync(cancellationToken);

        var created = await _festivalRepository.GetByIdAsync(festival.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<HeritageFestivalDto> UpdateAsync(Guid id, UpdateHeritageFestivalRequest request, CancellationToken cancellationToken)
    {
        var festival = await _festivalRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage festival not found.");

        await ValidateReferencesAsync(request.DistrictId, request.HeritagePlaceId, cancellationToken);

        if (request.EndDate < request.StartDate)
        {
            throw new ConflictException("End date cannot be earlier than start date.");
        }

        festival.Name = request.Name.Trim();
        festival.Description = request.Description.Trim();
        festival.DistrictId = request.DistrictId;
        festival.HeritagePlaceId = request.HeritagePlaceId;
        festival.StartDate = request.StartDate;
        festival.EndDate = request.EndDate;
        festival.IsRecurringAnnually = request.IsRecurringAnnually;
        festival.ImageUrl = request.ImageUrl?.Trim();
        festival.IsActive = request.IsActive;
        festival.UpdatedAt = DateTime.UtcNow;

        await _festivalRepository.SaveChangesAsync(cancellationToken);

        var updated = await _festivalRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var festival = await _festivalRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage festival not found.");

        _festivalRepository.Remove(festival);
        await _festivalRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateReferencesAsync(Guid districtId, Guid? heritagePlaceId, CancellationToken cancellationToken)
    {
        if (await _districtRepository.GetByIdAsync(districtId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        if (heritagePlaceId.HasValue && await _placeRepository.GetByIdAsync(heritagePlaceId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage place not found.");
        }
    }

    private static HeritageFestivalDto ToDto(HeritageFestival festival) => new()
    {
        Id = festival.Id,
        Name = festival.Name,
        Description = festival.Description,
        StartDate = festival.StartDate,
        EndDate = festival.EndDate,
        IsRecurringAnnually = festival.IsRecurringAnnually,
        ImageUrl = festival.ImageUrl,
        IsActive = festival.IsActive,
        DistrictId = festival.DistrictId,
        DistrictName = festival.District.Name,
        HeritagePlaceId = festival.HeritagePlaceId,
        HeritagePlaceName = festival.HeritagePlace?.Name,
        CreatedAt = festival.CreatedAt,
        UpdatedAt = festival.UpdatedAt,
    };
}
