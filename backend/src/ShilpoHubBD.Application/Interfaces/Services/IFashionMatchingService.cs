using ShilpoHubBD.Application.DTOs.AIShopping;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IFashionMatchingService
{
    Task<List<FashionMatchDto>> GetMatchesAsync(FashionMatchRequest request, CancellationToken cancellationToken);
}
