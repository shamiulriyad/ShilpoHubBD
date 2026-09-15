using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IDistrictService
{
    Task<List<DistrictDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<DistrictDto> UpdateAsync(Guid id, UpdateDistrictRequest request, CancellationToken cancellationToken);
}
