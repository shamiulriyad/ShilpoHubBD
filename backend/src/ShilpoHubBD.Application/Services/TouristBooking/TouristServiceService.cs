using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Application.Services.TouristBooking;

public class TouristServiceService : ITouristServiceService
{
    private readonly ITouristServiceRepository _serviceRepository;
    private readonly IDistrictRepository _districtRepository;

    public TouristServiceService(ITouristServiceRepository serviceRepository, IDistrictRepository districtRepository)
    {
        _serviceRepository = serviceRepository;
        _districtRepository = districtRepository;
    }

    public async Task<PagedResult<TouristServiceDto>> GetPagedAsync(TouristServiceQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _serviceRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<TouristServiceDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PagedResult<TouristServiceDto>> GetMineAsync(
        Guid producerId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _serviceRepository.GetPagedByProducerAsync(producerId, page, pageSize, cancellationToken);
        return new PagedResult<TouristServiceDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<TouristServiceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tourist service not found.");
        return ToDto(service);
    }

    public async Task<TouristServiceDto> CreateAsync(Guid producerId, CreateTouristServiceRequest request, CancellationToken cancellationToken)
    {
        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        var now = DateTime.UtcNow;
        var service = new Domain.Entities.TouristBooking.TouristService
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Type = request.Type,
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            DefaultCapacity = request.DefaultCapacity,
            Location = request.Location?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            DistrictId = request.DistrictId,
            ProducerId = producerId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _serviceRepository.AddAsync(service, cancellationToken);
        await _serviceRepository.SaveChangesAsync(cancellationToken);

        var created = await _serviceRepository.GetByIdAsync(service.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<TouristServiceDto> UpdateAsync(
        Guid producerId, bool isAdmin, Guid id, UpdateTouristServiceRequest request, CancellationToken cancellationToken)
    {
        var service = await GetOwnedServiceAsync(producerId, isAdmin, id, cancellationToken);

        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        service.Title = request.Title.Trim();
        service.Description = request.Description.Trim();
        service.Type = request.Type;
        service.Price = request.Price;
        service.DurationMinutes = request.DurationMinutes;
        service.DefaultCapacity = request.DefaultCapacity;
        service.Location = request.Location?.Trim();
        service.ImageUrl = request.ImageUrl?.Trim();
        service.DistrictId = request.DistrictId;
        service.IsActive = request.IsActive;
        service.UpdatedAt = DateTime.UtcNow;

        await _serviceRepository.SaveChangesAsync(cancellationToken);

        var updated = await _serviceRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task DeleteAsync(Guid producerId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var service = await GetOwnedServiceAsync(producerId, isAdmin, id, cancellationToken);

        _serviceRepository.Remove(service);
        await _serviceRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Domain.Entities.TouristBooking.TouristService> GetOwnedServiceAsync(
        Guid producerId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tourist service not found.");

        if (!isAdmin && service.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this service.");
        }

        return service;
    }

    private static TouristServiceDto ToDto(Domain.Entities.TouristBooking.TouristService service) => new()
    {
        Id = service.Id,
        Title = service.Title,
        Description = service.Description,
        Type = service.Type.ToString(),
        Price = service.Price,
        DurationMinutes = service.DurationMinutes,
        DefaultCapacity = service.DefaultCapacity,
        Location = service.Location,
        ImageUrl = service.ImageUrl,
        IsActive = service.IsActive,
        AverageRating = service.AverageRating,
        ReviewCount = service.ReviewCount,
        ProducerId = service.ProducerId,
        ProducerName = service.Producer.FullName,
        DistrictId = service.DistrictId,
        DistrictName = service.District.Name,
        CreatedAt = service.CreatedAt,
        UpdatedAt = service.UpdatedAt,
    };
}
