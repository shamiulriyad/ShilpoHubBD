using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageIdentity;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHeritageIdentityService
{
    Task<ProducerHeritageIdentityDto> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken);
    Task<PagedResult<ProducerHeritageIdentityDto>> GetVerifiedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<ProducerHeritageIdentityDto> UpsertAsync(Guid producerId, Guid currentUserId, bool isAdmin, UpsertHeritageIdentityRequest request, CancellationToken cancellationToken);
    Task<ProducerHeritageIdentityDto> VerifyAsync(Guid producerId, Guid verifierUserId, VerifyHeritageIdentityRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid producerId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);

    Task<LegacyScoreDto> GetScoreAsync(Guid producerId, CancellationToken cancellationToken);
    Task<PagedResult<LegacyScoreHistoryEntryDto>> GetScoreHistoryAsync(Guid producerId, int page, int pageSize, CancellationToken cancellationToken);
    Task<LegacyScoreDto> RecalculateScoreAsync(Guid producerId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}
