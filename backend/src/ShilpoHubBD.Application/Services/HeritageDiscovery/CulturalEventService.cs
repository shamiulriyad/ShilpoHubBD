using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Services.HeritageDiscovery;

public class CulturalEventService : ICulturalEventService
{
    private readonly ICulturalEventRepository _eventRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly IHeritagePlaceRepository _placeRepository;

    public CulturalEventService(
        ICulturalEventRepository eventRepository, IDistrictRepository districtRepository, IHeritagePlaceRepository placeRepository)
    {
        _eventRepository = eventRepository;
        _districtRepository = districtRepository;
        _placeRepository = placeRepository;
    }

    public async Task<PagedResult<CulturalEventDto>> GetPagedAsync(CulturalEventQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _eventRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<CulturalEventDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<CulturalEventDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var culturalEvent = await _eventRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Cultural event not found.");
        return ToDto(culturalEvent);
    }

    public async Task<CulturalEventDto> CreateAsync(CreateCulturalEventRequest request, CancellationToken cancellationToken)
    {
        await ValidateReferencesAsync(request.DistrictId, request.HeritagePlaceId, cancellationToken);

        if (request.EndDate.HasValue && request.EndDate < request.EventDate)
        {
            throw new ConflictException("End date cannot be earlier than the event date.");
        }

        var now = DateTime.UtcNow;
        var culturalEvent = new CulturalEvent
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            DistrictId = request.DistrictId,
            HeritagePlaceId = request.HeritagePlaceId,
            EventDate = request.EventDate,
            EndDate = request.EndDate,
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _eventRepository.AddAsync(culturalEvent, cancellationToken);
        await _eventRepository.SaveChangesAsync(cancellationToken);

        var created = await _eventRepository.GetByIdAsync(culturalEvent.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<CulturalEventDto> UpdateAsync(Guid id, UpdateCulturalEventRequest request, CancellationToken cancellationToken)
    {
        var culturalEvent = await _eventRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Cultural event not found.");

        await ValidateReferencesAsync(request.DistrictId, request.HeritagePlaceId, cancellationToken);

        if (request.EndDate.HasValue && request.EndDate < request.EventDate)
        {
            throw new ConflictException("End date cannot be earlier than the event date.");
        }

        culturalEvent.Name = request.Name.Trim();
        culturalEvent.Description = request.Description.Trim();
        culturalEvent.Category = request.Category.Trim();
        culturalEvent.DistrictId = request.DistrictId;
        culturalEvent.HeritagePlaceId = request.HeritagePlaceId;
        culturalEvent.EventDate = request.EventDate;
        culturalEvent.EndDate = request.EndDate;
        culturalEvent.ImageUrl = request.ImageUrl?.Trim();
        culturalEvent.IsActive = request.IsActive;
        culturalEvent.UpdatedAt = DateTime.UtcNow;

        await _eventRepository.SaveChangesAsync(cancellationToken);

        var updated = await _eventRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var culturalEvent = await _eventRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Cultural event not found.");

        _eventRepository.Remove(culturalEvent);
        await _eventRepository.SaveChangesAsync(cancellationToken);
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

    private static CulturalEventDto ToDto(CulturalEvent culturalEvent) => new()
    {
        Id = culturalEvent.Id,
        Name = culturalEvent.Name,
        Description = culturalEvent.Description,
        Category = culturalEvent.Category,
        EventDate = culturalEvent.EventDate,
        EndDate = culturalEvent.EndDate,
        ImageUrl = culturalEvent.ImageUrl,
        IsActive = culturalEvent.IsActive,
        DistrictId = culturalEvent.DistrictId,
        DistrictName = culturalEvent.District.Name,
        HeritagePlaceId = culturalEvent.HeritagePlaceId,
        HeritagePlaceName = culturalEvent.HeritagePlace?.Name,
        CreatedAt = culturalEvent.CreatedAt,
        UpdatedAt = culturalEvent.UpdatedAt,
    };
}
