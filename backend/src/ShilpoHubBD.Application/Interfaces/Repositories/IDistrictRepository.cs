using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IDistrictRepository
{
    Task<List<District>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<District?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
