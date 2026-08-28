using ShilpoHubBD.Application.DTOs.SupplierMatching;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISupplierMatchingService
{
    Task<List<SupplierMatchResultDto>> MatchAsync(SupplierMatchRequest request, CancellationToken cancellationToken);
}
