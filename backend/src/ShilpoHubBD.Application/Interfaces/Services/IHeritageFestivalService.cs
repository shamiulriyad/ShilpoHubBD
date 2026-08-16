using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHeritageFestivalService
{
    Task<PagedResult<HeritageFestivalDto>> GetPagedAsync(HeritageFestivalQueryParameters query, CancellationToken cancellationToken);
    Task<HeritageFestivalDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<HeritageFestivalDto> CreateAsync(CreateHeritageFestivalRequest request, CancellationToken cancellationToken);
    Task<HeritageFestivalDto> UpdateAsync(Guid id, UpdateHeritageFestivalRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
