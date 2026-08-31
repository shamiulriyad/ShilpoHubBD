using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Application.Services.ArVr;

public class VillageTourService : IVillageTourService
{
    private readonly IVillageTourStopRepository _stopRepository;
    private readonly IHeritagePlaceRepository _heritagePlaceRepository;

    public VillageTourService(IVillageTourStopRepository stopRepository, IHeritagePlaceRepository heritagePlaceRepository)
    {
        _stopRepository = stopRepository;
        _heritagePlaceRepository = heritagePlaceRepository;
    }

    public async Task<PagedResult<VillageTourStopDto>> GetPagedAsync(VillageTourStopQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _stopRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<VillageTourStopDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<VillageTourStopDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var stop = await _stopRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Village tour stop not found.");
        return ToDto(stop);
    }

    public async Task<VillageTourStopDto> CreateAsync(CreateVillageTourStopRequest request, CancellationToken cancellationToken)
    {
        if (await _heritagePlaceRepository.GetByIdAsync(request.HeritagePlaceId, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage place not found.");
        }

        var now = DateTime.UtcNow;
        var stop = new VillageTourStop
        {
            Id = Guid.NewGuid(),
            HeritagePlaceId = request.HeritagePlaceId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            MediaUrl = request.MediaUrl.Trim(),
            MediaType = request.MediaType,
            ThumbnailUrl = request.ThumbnailUrl?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _stopRepository.AddAsync(stop, cancellationToken);
        await _stopRepository.SaveChangesAsync(cancellationToken);

        var created = await _stopRepository.GetByIdAsync(stop.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<VillageTourStopDto> UpdateAsync(Guid id, UpdateVillageTourStopRequest request, CancellationToken cancellationToken)
    {
        var stop = await _stopRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Village tour stop not found.");

        stop.Title = request.Title.Trim();
        stop.Description = request.Description?.Trim();
        stop.MediaUrl = request.MediaUrl.Trim();
        stop.MediaType = request.MediaType;
        stop.ThumbnailUrl = request.ThumbnailUrl?.Trim();
        stop.DisplayOrder = request.DisplayOrder;
        stop.IsActive = request.IsActive;
        stop.UpdatedAt = DateTime.UtcNow;

        await _stopRepository.SaveChangesAsync(cancellationToken);

        var updated = await _stopRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var stop = await _stopRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Village tour stop not found.");

        _stopRepository.Remove(stop);
        await _stopRepository.SaveChangesAsync(cancellationToken);
    }

    private static VillageTourStopDto ToDto(VillageTourStop stop) => new()
    {
        Id = stop.Id,
        Title = stop.Title,
        Description = stop.Description,
        MediaUrl = stop.MediaUrl,
        MediaType = stop.MediaType.ToString(),
        ThumbnailUrl = stop.ThumbnailUrl,
        DisplayOrder = stop.DisplayOrder,
        IsActive = stop.IsActive,
        HeritagePlaceId = stop.HeritagePlaceId,
        HeritagePlaceName = stop.HeritagePlace.Name,
        CreatedAt = stop.CreatedAt,
        UpdatedAt = stop.UpdatedAt,
    };
}
