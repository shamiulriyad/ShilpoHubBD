using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.TouristBooking;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ITouristServiceService
{
    Task<PagedResult<TouristServiceDto>> GetPagedAsync(TouristServiceQueryParameters query, CancellationToken cancellationToken);

    Task<PagedResult<TouristServiceDto>> GetMineAsync(
        Guid producerId, int page, int pageSize, CancellationToken cancellationToken);

    Task<TouristServiceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<TouristServiceDto> CreateAsync(Guid producerId, CreateTouristServiceRequest request, CancellationToken cancellationToken);

    Task<TouristServiceDto> UpdateAsync(
        Guid producerId, bool isAdmin, Guid id, UpdateTouristServiceRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid producerId, bool isAdmin, Guid id, CancellationToken cancellationToken);
}
