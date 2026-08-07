using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IDistrictService
{
    Task<List<DistrictDto>> GetAllAsync(CancellationToken cancellationToken);
}
