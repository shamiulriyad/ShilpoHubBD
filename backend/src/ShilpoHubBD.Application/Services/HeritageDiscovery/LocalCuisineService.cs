using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Services.HeritageDiscovery;

public class LocalCuisineService : ILocalCuisineService
{
    private readonly ILocalCuisineRepository _cuisineRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly IHeritagePlaceRepository _placeRepository;

    public LocalCuisineService(
        ILocalCuisineRepository cuisineRepository, IDistrictRepository districtRepository, IHeritagePlaceRepository placeRepository)
    {
        _cuisineRepository = cuisineRepository;
        _districtRepository = districtRepository;
        _placeRepository = placeRepository;
    }

    public async Task<PagedResult<LocalCuisineDto>> GetPagedAsync(LocalCuisineQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _cuisineRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<LocalCuisineDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<LocalCuisineDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var cuisine = await _cuisineRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Local cuisine entry not found.");
        return ToDto(cuisine);
    }

    public async Task<LocalCuisineDto> CreateAsync(CreateLocalCuisineRequest request, CancellationToken cancellationToken)
    {
        await ValidateReferencesAsync(request.DistrictId, request.HeritagePlaceId, cancellationToken);

        var now = DateTime.UtcNow;
        var cuisine = new LocalCuisine
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            DistrictId = request.DistrictId,
            HeritagePlaceId = request.HeritagePlaceId,
            WhereToTry = request.WhereToTry?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _cuisineRepository.AddAsync(cuisine, cancellationToken);
        await _cuisineRepository.SaveChangesAsync(cancellationToken);

        var created = await _cuisineRepository.GetByIdAsync(cuisine.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<LocalCuisineDto> UpdateAsync(Guid id, UpdateLocalCuisineRequest request, CancellationToken cancellationToken)
    {
        var cuisine = await _cuisineRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Local cuisine entry not found.");

        await ValidateReferencesAsync(request.DistrictId, request.HeritagePlaceId, cancellationToken);

        cuisine.Name = request.Name.Trim();
        cuisine.Description = request.Description.Trim();
        cuisine.DistrictId = request.DistrictId;
        cuisine.HeritagePlaceId = request.HeritagePlaceId;
        cuisine.WhereToTry = request.WhereToTry?.Trim();
        cuisine.ImageUrl = request.ImageUrl?.Trim();
        cuisine.IsActive = request.IsActive;
        cuisine.UpdatedAt = DateTime.UtcNow;

        await _cuisineRepository.SaveChangesAsync(cancellationToken);

        var updated = await _cuisineRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var cuisine = await _cuisineRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Local cuisine entry not found.");

        _cuisineRepository.Remove(cuisine);
        await _cuisineRepository.SaveChangesAsync(cancellationToken);
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

    private static LocalCuisineDto ToDto(LocalCuisine cuisine) => new()
    {
        Id = cuisine.Id,
        Name = cuisine.Name,
        Description = cuisine.Description,
        WhereToTry = cuisine.WhereToTry,
        ImageUrl = cuisine.ImageUrl,
        IsActive = cuisine.IsActive,
        DistrictId = cuisine.DistrictId,
        DistrictName = cuisine.District.Name,
        HeritagePlaceId = cuisine.HeritagePlaceId,
        HeritagePlaceName = cuisine.HeritagePlace?.Name,
        CreatedAt = cuisine.CreatedAt,
        UpdatedAt = cuisine.UpdatedAt,
    };
}
