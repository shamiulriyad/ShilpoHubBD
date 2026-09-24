using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Application.Services.Tourism;

public class TourismLocationService : ITourismLocationService
{
    private readonly ITourismLocationRepository _locationRepository;
    private readonly IDistrictRepository _districtRepository;

    public TourismLocationService(ITourismLocationRepository locationRepository, IDistrictRepository districtRepository)
    {
        _locationRepository = locationRepository;
        _districtRepository = districtRepository;
    }

    public async Task<PagedResult<TourismLocationDto>> GetPagedAsync(TourismLocationQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _locationRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<TourismLocationDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<TourismLocationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tourism location not found.");
        return ToDto(location);
    }

    public async Task<TourismLocationDto> CreateAsync(CreateTourismLocationRequest request, CancellationToken cancellationToken)
    {
        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        var now = DateTime.UtcNow;
        var location = new TourismLocation
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Type = request.Type,
            Description = request.Description.Trim(),
            DistrictId = request.DistrictId,
            Address = request.Address?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Price = request.Price,
            EntryFee = request.EntryFee,
            OpeningHours = request.OpeningHours?.Trim(),
            ContactInfo = request.ContactInfo?.Trim(),
            Facilities = request.Facilities?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            IsVerified = false,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _locationRepository.AddAsync(location, cancellationToken);
        await _locationRepository.SaveChangesAsync(cancellationToken);

        var created = await _locationRepository.GetByIdAsync(location.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<TourismLocationDto> UpdateAsync(Guid id, UpdateTourismLocationRequest request, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tourism location not found.");

        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        location.Name = request.Name.Trim();
        location.Type = request.Type;
        location.Description = request.Description.Trim();
        location.DistrictId = request.DistrictId;
        location.Address = request.Address?.Trim();
        location.Latitude = request.Latitude;
        location.Longitude = request.Longitude;
        location.Price = request.Price;
        location.EntryFee = request.EntryFee;
        location.OpeningHours = request.OpeningHours?.Trim();
        location.ContactInfo = request.ContactInfo?.Trim();
        location.Facilities = request.Facilities?.Trim();
        location.ImageUrl = request.ImageUrl?.Trim();
        location.IsActive = request.IsActive;
        location.IsVerified = request.IsVerified;
        location.UpdatedAt = DateTime.UtcNow;

        await _locationRepository.SaveChangesAsync(cancellationToken);

        var updated = await _locationRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tourism location not found.");

        _locationRepository.Remove(location);
        await _locationRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<TourismLocationDto> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tourism location not found.");

        location.IsActive = isActive;
        location.UpdatedAt = DateTime.UtcNow;
        await _locationRepository.SaveChangesAsync(cancellationToken);

        return ToDto(location);
    }

    public async Task<TourismLocationDto> SetVerificationAsync(Guid id, bool isVerified, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tourism location not found.");

        location.IsVerified = isVerified;
        location.UpdatedAt = DateTime.UtcNow;
        await _locationRepository.SaveChangesAsync(cancellationToken);

        return ToDto(location);
    }

    private static TourismLocationDto ToDto(TourismLocation location) => new()
    {
        Id = location.Id,
        Name = location.Name,
        Type = location.Type.ToString(),
        Description = location.Description,
        Address = location.Address,
        Latitude = location.Latitude,
        Longitude = location.Longitude,
        Price = location.Price,
        EntryFee = location.EntryFee,
        OpeningHours = location.OpeningHours,
        ContactInfo = location.ContactInfo,
        Facilities = location.Facilities,
        ImageUrl = location.ImageUrl,
        ImageCredit = location.ImageCredit,
        ImageSourceUrl = location.ImageSourceUrl,
        IsVerified = location.IsVerified,
        IsActive = location.IsActive,
        Area = location.Area,
        SourceUrl = location.SourceUrl,
        VerificationStatus = location.VerificationStatus,
        UnverifiedFields = location.UnverifiedFields,
        CoordinatesSource = location.CoordinatesSource,
        CoordinatesPrecision = location.CoordinatesPrecision,
        DataRetrievedOn = location.DataRetrievedOn,
        DistrictId = location.DistrictId,
        DistrictName = location.District.Name,
        CreatedAt = location.CreatedAt,
        UpdatedAt = location.UpdatedAt,
    };
}
