using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Services.HeritageDiscovery;

public class HeritagePlaceService : IHeritagePlaceService
{
    private const double EarthRadiusKm = 6371.0;

    private readonly IHeritagePlaceRepository _placeRepository;
    private readonly IDistrictRepository _districtRepository;

    public HeritagePlaceService(IHeritagePlaceRepository placeRepository, IDistrictRepository districtRepository)
    {
        _placeRepository = placeRepository;
        _districtRepository = districtRepository;
    }

    public async Task<PagedResult<HeritagePlaceDto>> GetPagedAsync(HeritagePlaceQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _placeRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<HeritagePlaceDto>
        {
            Items = items.Select(p => ToDto(p, null)).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PagedResult<HeritagePlaceDto>> GetNearbyAsync(NearbyHeritagePlaceQueryParameters query, CancellationToken cancellationToken)
    {
        var latDelta = query.RadiusKm / 111.0;
        var lngDelta = query.RadiusKm / (111.0 * Math.Max(0.01, Math.Cos(ToRadians(query.Latitude))));

        var candidates = await _placeRepository.GetActiveWithinBoundsAsync(
            query.Latitude - latDelta,
            query.Latitude + latDelta,
            query.Longitude - lngDelta,
            query.Longitude + lngDelta,
            cancellationToken);

        var withDistance = candidates
            .Select(p => (Place: p, DistanceKm: HaversineDistanceKm(query.Latitude, query.Longitude, p.Latitude, p.Longitude)))
            .Where(x => x.DistanceKm <= query.RadiusKm)
            .OrderBy(x => x.DistanceKm)
            .ToList();

        var totalCount = withDistance.Count;
        var page = withDistance
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new PagedResult<HeritagePlaceDto>
        {
            Items = page.Select(x => ToDto(x.Place, x.DistanceKm)).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<HeritagePlaceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var place = await _placeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage place not found.");
        return ToDto(place, null);
    }

    public async Task<HeritagePlaceDto> CreateAsync(CreateHeritagePlaceRequest request, CancellationToken cancellationToken)
    {
        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        var now = DateTime.UtcNow;
        var place = new HeritagePlace
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            PlaceType = request.PlaceType,
            DistrictId = request.DistrictId,
            Address = request.Address?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ImageUrl = request.ImageUrl?.Trim(),
            IsFeatured = request.IsFeatured,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _placeRepository.AddAsync(place, cancellationToken);
        await _placeRepository.SaveChangesAsync(cancellationToken);

        var created = await _placeRepository.GetByIdAsync(place.Id, cancellationToken);
        return ToDto(created!, null);
    }

    public async Task<HeritagePlaceDto> UpdateAsync(Guid id, UpdateHeritagePlaceRequest request, CancellationToken cancellationToken)
    {
        var place = await _placeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage place not found.");

        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        place.Name = request.Name.Trim();
        place.Description = request.Description.Trim();
        place.PlaceType = request.PlaceType;
        place.DistrictId = request.DistrictId;
        place.Address = request.Address?.Trim();
        place.Latitude = request.Latitude;
        place.Longitude = request.Longitude;
        place.ImageUrl = request.ImageUrl?.Trim();
        place.IsFeatured = request.IsFeatured;
        place.IsActive = request.IsActive;
        place.UpdatedAt = DateTime.UtcNow;

        await _placeRepository.SaveChangesAsync(cancellationToken);

        var updated = await _placeRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!, null);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var place = await _placeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage place not found.");

        _placeRepository.Remove(place);
        await _placeRepository.SaveChangesAsync(cancellationToken);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static HeritagePlaceDto ToDto(HeritagePlace place, double? distanceKm) => new()
    {
        Id = place.Id,
        Name = place.Name,
        Description = place.Description,
        PlaceType = place.PlaceType.ToString(),
        Address = place.Address,
        Latitude = place.Latitude,
        Longitude = place.Longitude,
        ImageUrl = place.ImageUrl,
        IsFeatured = place.IsFeatured,
        IsActive = place.IsActive,
        AverageRating = place.AverageRating,
        ReviewCount = place.ReviewCount,
        DistrictId = place.DistrictId,
        DistrictName = place.District.Name,
        DistanceKm = distanceKm.HasValue ? Math.Round(distanceKm.Value, 2) : null,
        CreatedAt = place.CreatedAt,
        UpdatedAt = place.UpdatedAt,
    };
}
