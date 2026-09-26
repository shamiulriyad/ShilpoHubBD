using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICraftHeritageService
{
    Task<List<CraftHeritageEntryDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<CraftHeritageEntryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CraftHeritageEntryDto> CreateAsync(SaveCraftHeritageEntryRequest request, CancellationToken cancellationToken);
    Task<CraftHeritageEntryDto> UpdateAsync(Guid id, SaveCraftHeritageEntryRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
