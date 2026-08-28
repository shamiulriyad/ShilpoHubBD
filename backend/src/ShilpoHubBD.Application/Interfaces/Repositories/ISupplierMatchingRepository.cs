using ShilpoHubBD.Application.DTOs.SupplierMatching;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ISupplierMatchingRepository
{
    Task<List<SupplierMatchCandidateDto>> GetCandidatesAsync(SupplierMatchRequest request, CancellationToken cancellationToken);
}
