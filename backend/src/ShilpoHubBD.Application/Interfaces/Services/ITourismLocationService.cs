using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Tourism;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ITourismLocationService
{
    Task<PagedResult<TourismLocationDto>> GetPagedAsync(TourismLocationQueryParameters query, CancellationToken cancellationToken);
    Task<TourismLocationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TourismLocationDto> CreateAsync(CreateTourismLocationRequest request, CancellationToken cancellationToken);
    Task<TourismLocationDto> UpdateAsync(Guid id, UpdateTourismLocationRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<TourismLocationDto> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task<TourismLocationDto> SetVerificationAsync(Guid id, bool isVerified, CancellationToken cancellationToken);
}
