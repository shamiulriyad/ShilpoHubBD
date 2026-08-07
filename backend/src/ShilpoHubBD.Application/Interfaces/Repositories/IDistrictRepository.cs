using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IDistrictRepository
{
    Task<List<District>> GetAllAsync(CancellationToken cancellationToken);
    Task<District?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
